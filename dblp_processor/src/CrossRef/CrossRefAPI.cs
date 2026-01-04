using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DataProcessor
{

    public sealed class RateGate : IAsyncDisposable
    {
        private readonly Channel<int> _tokens;
        private readonly PeriodicTimer _timer;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _pump;
        private readonly int _tokensPerSecond;

        public RateGate(int tokensPerSecond, int burst)
        {
            _tokensPerSecond = tokensPerSecond;
            _tokens = Channel.CreateBounded<int>(new BoundedChannelOptions(burst)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = false
            });

            for (int i = 0; i < burst; i++) _tokens.Writer.TryWrite(1);

            _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            _pump = PumpAsync();
        }

        private async Task PumpAsync()
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(_cts.Token))
                    for (int i = 0; i < _tokensPerSecond; i++)
                        _tokens.Writer.TryWrite(1);
            }
            catch (OperationCanceledException) { }
        }

        public ValueTask<int> WaitAsync(CancellationToken ct)
        => _tokens.Reader.ReadAsync(ct);

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _timer.Dispose();
            try { await _pump; } catch { }
            _cts.Dispose();
        }
    }

    public static class CrossrefBulk
    {
        private static readonly HttpClient Http = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        })
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // 1 DOI の JSON を「文字列で」取得
        public static async Task<string?> GetWorkJsonByDoiAsync(string doi, string mailto, CancellationToken ct)
        {
            var url = $"https://api.crossref.org/works/{Uri.EscapeDataString(doi)}?mailto={Uri.EscapeDataString(mailto)}";

            using var resp = await Http.GetAsync(url, ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            if ((int)resp.StatusCode == 429)
            {
                var delay = resp.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(1);
                await Task.Delay(delay, ct);
                return await GetWorkJsonByDoiAsync(doi, mailto, ct);
            }

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync(ct); // ← JSON文字列そのまま
        }

        // 複数 DOI をまとめて取得（JSON文字列を返す）
        public static async Task<Dictionary<string, string?>> GetManyAsync(
            IEnumerable<string> dois,
            string mailto,
            int tokensPerSecond = 3,  // polite pool目安
            int concurrency = 3,       // 安全寄り
            CancellationToken ct = default)
        {
            var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            await using var rate = new RateGate(tokensPerSecond, burst: tokensPerSecond);
            using var sem = new SemaphoreSlim(concurrency, concurrency);

            var tasks = new List<Task>();

            var maxCounter = dois.Count();
            var counter = 0;

            foreach (var raw in dois)
            {
                var doi = raw?.Trim();
                if (string.IsNullOrEmpty(doi)) continue;

                tasks.Add(Task.Run(async () =>
                {
                    await sem.WaitAsync(ct);
                    try
                    {
                        await rate.WaitAsync(ct); // レート制限
                        var json = await GetWorkJsonByDoiAsync(doi, mailto, ct);
                        lock (result) result[doi] = json; // nullならNotFound
                        lock (result) counter++;

                        if (json == null)
                        {
                            Console.WriteLine("\t Not found: " + doi + " / " + counter + " / " + maxCounter);

                        }
                        else
                        {
                            Console.WriteLine("\t Found: " + doi + " / " + counter + " / " + maxCounter);
                        }
                    }
                    finally
                    {
                        sem.Release();
                    }
                }, ct));
            }

            await Task.WhenAll(tasks);
            return result;
        }


    }
}