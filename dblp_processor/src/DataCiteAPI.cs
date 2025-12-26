using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;
namespace DataProcessor
{

    public sealed class DataCiteRateGate
    {
        private readonly TimeSpan _minInterval;
        private readonly SemaphoreSlim _mutex = new(1, 1);
        private DateTimeOffset _next = DateTimeOffset.MinValue;

        public DataCiteRateGate(TimeSpan minInterval) => _minInterval = minInterval;

        public async Task WaitAsync(CancellationToken ct)
        {
            await _mutex.WaitAsync(ct);
            try
            {
                var now = DateTimeOffset.UtcNow;
                if (now < _next) await Task.Delay(_next - now, ct);
                _next = DateTimeOffset.UtcNow + _minInterval;
            }
            finally { _mutex.Release(); }
        }
    }

    public static class DataCiteBatch
    {
        public static async Task<IReadOnlyDictionary<string, JsonDocument?>> GetDoisAsync(
            HttpClient http,
            IEnumerable<string> dois,
            int maxConcurrency = 4,
            double requestsPerSecond = 2.5,   // Identified(1000/5min ≒3.33/s)より少し控えめ、など
            int maxRetries = 5,
            CancellationToken ct = default)
        {
            var gate = new DataCiteRateGate(TimeSpan.FromSeconds(1.0 / requestsPerSecond));
            var sem = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            var results = new ConcurrentDictionary<string, JsonDocument?>();

            var tasks = dois.Select(async doi =>
            {
                await sem.WaitAsync(ct);
                try
                {

                    results[doi] = await GetOneWithRetryAsync(http, gate, doi, maxRetries, ct);
                }
                finally { sem.Release(); }
            });

            await Task.WhenAll(tasks);
            return results;
        }

        private static async Task<JsonDocument?> GetOneWithRetryAsync(
            HttpClient http,
            DataCiteRateGate gate,
            string doi,
            int maxRetries,
            CancellationToken ct)
        {
            Console.WriteLine("Getting DOI: " + doi);
            for (int attempt = 0; ; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                await gate.WaitAsync(ct);
                var encoded = Uri.EscapeDataString(doi);
                using var req = new HttpRequestMessage(HttpMethod.Get, $"dois/{encoded}");

                using var res = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

                if (res.StatusCode == HttpStatusCode.NotFound) return null; // 404:contentReference[oaicite:5]{index=5}

                if (res.StatusCode == (HttpStatusCode)429) // Too Many Requests:contentReference[oaicite:6]{index=6}
                {
                    if (attempt >= maxRetries) res.EnsureSuccessStatusCode();

                    // Retry-After があればそれに従う（無ければ指数バックオフ）
                    var delay = res.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, attempt)));
                    await Task.Delay(delay, ct);
                    continue;
                }

                // 一時エラー系はバックオフして再試行
                if (res.StatusCode is HttpStatusCode.BadGateway
                    or HttpStatusCode.ServiceUnavailable
                    or HttpStatusCode.GatewayTimeout)
                {
                    if (attempt >= maxRetries) res.EnsureSuccessStatusCode();
                    await Task.Delay(TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, attempt))), ct);
                    continue;
                }

                res.EnsureSuccessStatusCode();
                await using var stream = await res.Content.ReadAsStreamAsync(ct);
                return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            }
        }
    }

    public static class DataCiteClient
    {
        public static HttpClient CreateHttpClient(string contactEmail)
        {
            var http = new HttpClient
            {
                BaseAddress = new Uri("https://api.datacite.org/"),
                Timeout = TimeSpan.FromSeconds(30),
            };

            // DataCiteは頻繁に叩く場合 User-Agent に連絡先(mailto)を入れることを推奨
            // これにより "Identified" 扱いになり、レート上限が上がります。:contentReference[oaicite:2]{index=2}
            http.DefaultRequestHeaders.UserAgent.ParseAdd(
                $"MyTool/1.0 (+mailto:{contactEmail})");

            http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));

            return http;
        }

        public static async Task<JsonDocument?> GetDoiAsync(
            HttpClient http, string doi, CancellationToken ct = default)
        {
            // DOIには "/" があるので必ず URL エンコード
            var encoded = Uri.EscapeDataString(doi);
            using var req = new HttpRequestMessage(HttpMethod.Get, $"dois/{encoded}");

            using var res = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

            if (res.StatusCode == HttpStatusCode.NotFound) return null; // DOIが存在しない(404):contentReference[oaicite:3]{index=3}
            res.EnsureSuccessStatusCode();

            await using var stream = await res.Content.ReadAsStreamAsync(ct);
            return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        }
    }
}