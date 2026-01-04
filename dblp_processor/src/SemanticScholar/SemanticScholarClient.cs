using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public static class SemanticScholarClient
{
    public static async Task<string[]> downloadReferrence(string[] dois)
    {
        if (dois == null) throw new ArgumentNullException(nameof(dois));

        var apiKey = Environment.GetEnvironmentVariable("SEMANTIC_SCHOLAR_API_KEY");

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        if (!string.IsNullOrWhiteSpace(apiKey))
            http.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var results = new string[dois.Length];
        for (int i = 0; i < dois.Length; i++)
        {
            results[i] = $"{{ \"inputDoi\": \"{dois[i]}\", \"error\": \"empty_doi\" }}";
        }

        // 空 DOI はAPIを呼ばずに返す
            var pending = new List<int>();
        for (int i = 0; i < dois.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(dois[i]))
                results[i] = JsonSerializer.Serialize(new { inputDoi = dois[i] ?? "", error = "empty_doi" });
            else
                pending.Add(i);
        }

        // batch は最大500 ids。10MBや失敗回避のため小さめに分割（必要なら調整）
        const int batchSize = 100;

        var needPaging = new List<int>();

        for (int start = 0; start < pending.Count; start += batchSize)
        {
            var slice = pending.Skip(start).Take(batchSize).ToArray();
            var ids = slice.Select(idx => "DOI:" + dois[idx].Trim()).ToArray();

            Console.WriteLine("Executing SemanticScholarClient: " + start + " / " + pending.Count);
            await Task.Delay(500); 

            JsonDocument? doc = null;
            try
            {
                doc = await FetchBatchJsonAsync(http, ids, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // batch が落ちたらスライス全部を paging にフォールバック
                foreach (var idx in slice)
                {
                    needPaging.Add(idx);
                    // ここはログ用途（最終的には paging 結果で上書きされる想定）
                    results[idx] = JsonSerializer.Serialize(new
                    {
                        inputDoi = dois[idx].Trim(),
                        warning = "batch_failed_fallback_to_paging",
                        exception = ex.Message
                    });
                }
                continue;
            }

            using (doc)
            {
                // batchレスポンスは JSON配列： [ { ... }, null, { ... }, ... ]
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Array)
                {
                    foreach (var idx in slice) needPaging.Add(idx);
                    continue;
                }

                // DOIで引ける場合に備えて、externalIds.DOI -> item の辞書も作る
                var byDoi = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in root.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;
                    if (TryGetDoiFromBatchItem(item, out var itemDoi))
                    {
                        var key = NormalizeDoi(itemDoi);
                        if (!byDoi.ContainsKey(key))
                            byDoi[key] = item;
                    }
                }

                int j = 0;
                foreach (var idx in slice)
                {
                    var inputDoi = dois[idx].Trim();
                    var key = NormalizeDoi(inputDoi);

                    JsonElement item;
                    bool found = false;

                    if (byDoi.TryGetValue(key, out item))
                    {
                        found = true;
                    }
                    else
                    {
                        // 同順フォールバック
                        if (j < root.GetArrayLength())
                        {
                            item = root[j];
                            found = item.ValueKind == JsonValueKind.Object;
                        }
                        else
                        {
                            found = false;
                            item = default;
                        }
                    }
                    j++;

                    if (!found)
                    {
                        results[idx] = JsonSerializer.Serialize(new
                        {
                            inputDoi = inputDoi,
                            error = "not_found_or_unavailable_in_batch"
                        });
                        continue;
                    }

                    int? referenceCount = null;
                    if (item.TryGetProperty("referenceCount", out var rcEl) && rcEl.ValueKind == JsonValueKind.Number)
                        referenceCount = rcEl.GetInt32();

                    bool hasReferences = item.TryGetProperty("references", out var refsEl) && refsEl.ValueKind == JsonValueKind.Array;
                    int refsLen = hasReferences ? refsEl.GetArrayLength() : 0;

                    // batchで完結できる条件（最大1000件まで）
                    bool completeByBatch =
                        (referenceCount.HasValue && referenceCount.Value <= 1000 && hasReferences) ||
                        (!referenceCount.HasValue && hasReferences && refsLen < 1000);

                    if (completeByBatch)
                    {
                        results[idx] = BuildReferencesEndpointLikeJsonFromBatch(
                            inputDoi,
                            refsEl,
                            total: referenceCount
                        );
                    }
                    else
                    {
                        needPaging.Add(idx);
                    }
                }
            }
        }

        /*
        // pagingで全件回収（referenceCount>1000 など）
        foreach (var idx in needPaging)
        {
            results[idx] = await FetchAllReferencesExternalIdsJsonAsync(http, dois[idx].Trim(), CancellationToken.None);
        }
        */

        return results;
    }

    // -------------------- batch（DTOなし） --------------------

    private static async Task<JsonDocument> FetchBatchJsonAsync(HttpClient http, string[] ids, CancellationToken ct)
    {
        // 最小限：referenceCount と references.externalIds と externalIds(自分のDOI照合用)
        var url = "https://api.semanticscholar.org/graph/v1/paper/batch?fields=referenceCount,references.externalIds,externalIds";

        var payload = JsonSerializer.Serialize(new { ids });
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var resp = await SendWithRetryAsync(http, req, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"batch failed: {(int)resp.StatusCode} {resp.StatusCode} body={body}");
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        return await JsonDocument.ParseAsync(stream, cancellationToken: ct);
    }

    private static bool TryGetDoiFromBatchItem(JsonElement item, out string doi)
    {
        doi = "";
        if (item.TryGetProperty("externalIds", out var exIds) && exIds.ValueKind == JsonValueKind.Object)
        {
            if (exIds.TryGetProperty("DOI", out var doiEl) && doiEl.ValueKind == JsonValueKind.String)
            {
                doi = doiEl.GetString() ?? "";
                return doi.Length > 0;
            }
        }
        return false;
    }

    private static string BuildReferencesEndpointLikeJsonFromBatch(string inputDoi, JsonElement referencesArray, int? total)
    {
        using var ms = new MemoryStream();
        using (var w = new Utf8JsonWriter(ms))
        {
            w.WriteStartObject();
            w.WriteString("inputDoi", inputDoi);
            if (total.HasValue) w.WriteNumber("total", total.Value);

            w.WritePropertyName("data");
            w.WriteStartArray();

            // batchの references は「Paper 配列」で、各要素に externalIds がある（ことが多い）
            foreach (var refPaper in referencesArray.EnumerateArray())
            {
                w.WriteStartObject();
                w.WritePropertyName("citedPaper");
                w.WriteStartObject();

                w.WritePropertyName("externalIds");
                if (refPaper.ValueKind == JsonValueKind.Object &&
                    refPaper.TryGetProperty("externalIds", out var ex) &&
                    ex.ValueKind == JsonValueKind.Object)
                {
                    ex.WriteTo(w); // 型が何であってもそのままコピー
                }
                else
                {
                    w.WriteStartObject();
                    w.WriteEndObject();
                }

                w.WriteEndObject(); // citedPaper
                w.WriteEndObject(); // item
            }

            w.WriteEndArray(); // data
            w.WriteNull("next");
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    // -------------------- paging /references（DTOなし・ストリーミング） --------------------

    private static async Task<string> FetchAllReferencesExternalIdsJsonAsync(HttpClient http, string doi, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(doi))
            return JsonSerializer.Serialize(new { inputDoi = doi ?? "", error = "empty_doi" });

        const int limit = 1000;
        int offset = 0;

        // 1ページ目を先に取って total/next を把握し、Writerを開始する
        var firstUrl = BuildReferencesUrl(doi, limit, offset);

        using var firstReq = new HttpRequestMessage(HttpMethod.Get, firstUrl);
        firstReq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var firstResp = await SendWithRetryAsync(http, firstReq, ct);

        if (firstResp.StatusCode == HttpStatusCode.NotFound)
            return JsonSerializer.Serialize(new { inputDoi = doi, error = "not_found", status = (int)firstResp.StatusCode });

        if (!firstResp.IsSuccessStatusCode)
        {
            var body = await firstResp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Serialize(new { inputDoi = doi, error = "http_error", status = (int)firstResp.StatusCode, responseBody = body });
        }

        await using var firstStream = await firstResp.Content.ReadAsStreamAsync(ct);
        using var firstDoc = await JsonDocument.ParseAsync(firstStream, cancellationToken: ct);
        var firstRoot = firstDoc.RootElement;

        int? total = null;
        if (firstRoot.TryGetProperty("total", out var totalEl) && totalEl.ValueKind == JsonValueKind.Number)
            total = totalEl.GetInt32();

        int? next = null;
        if (firstRoot.TryGetProperty("next", out var nextEl) && nextEl.ValueKind == JsonValueKind.Number)
            next = nextEl.GetInt32();

        using var ms = new MemoryStream();
        using var w = new Utf8JsonWriter(ms);

        w.WriteStartObject();
        w.WriteString("inputDoi", doi);
        if (total.HasValue) w.WriteNumber("total", total.Value);

        w.WritePropertyName("data");
        w.WriteStartArray();

        // 1ページ目の data を出力
        WriteDataArrayItems(firstRoot, w);

        // 2ページ目以降
        while (next.HasValue)
        {
            offset = next.Value;
            var url = BuildReferencesUrl(doi, limit, offset);

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var resp = await SendWithRetryAsync(http, req, ct);

            if (!resp.IsSuccessStatusCode)
            {
                // 途中で失敗した場合も「途中まで」の JSON を閉じて返す（デバッグしやすい）
                w.WriteEndArray();
                w.WriteNull("next");
                w.WriteString("warning", "paging_incomplete_due_to_http_error");
                w.WriteNumber("status", (int)resp.StatusCode);
                w.WriteEndObject();
                w.Flush();
                return Encoding.UTF8.GetString(ms.ToArray());
            }

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = doc.RootElement;

            WriteDataArrayItems(root, w);

            if (root.TryGetProperty("next", out var nEl) && nEl.ValueKind == JsonValueKind.Number)
                next = nEl.GetInt32();
            else
                next = null;
        }

        w.WriteEndArray();
        w.WriteNull("next");
        w.WriteEndObject();
        w.Flush();

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static void WriteDataArrayItems(JsonElement root, Utf8JsonWriter w)
    {
        if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in dataEl.EnumerateArray())
            {
                // item はこのdocの寿命内で WriteTo すればOK（親問題も起きない）
                item.WriteTo(w);
            }
        }
    }

    private static string BuildReferencesUrl(string doi, int limit, int offset)
    {
        return "https://api.semanticscholar.org/graph/v1/paper/DOI:" + Uri.EscapeDataString(doi) +
               "/references?fields=externalIds" +
               "&limit=" + limit +
               "&offset=" + offset;
    }

    private static string NormalizeDoi(string doi)
        => (doi ?? "").Trim().ToLowerInvariant();

    // -------------------- 429対策：簡易リトライ（Retry-After優先） --------------------

    private static async Task<HttpResponseMessage> SendWithRetryAsync(HttpClient http, HttpRequestMessage req, CancellationToken ct)
    {
        // HttpRequestMessage は再送に使い回せないので clone する
        static HttpRequestMessage Clone(HttpRequestMessage r)
        {
            var c = new HttpRequestMessage(r.Method, r.RequestUri);

            foreach (var h in r.Headers)
                c.Headers.TryAddWithoutValidation(h.Key, h.Value);

            if (r.Content != null)
            {
                var bytes = r.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                c.Content = new ByteArrayContent(bytes);
                foreach (var h in r.Content.Headers)
                    c.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }
            return c;
        }

        const int maxAttempts = 5;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var r = Clone(req);
            var resp = await http.SendAsync(r, HttpCompletionOption.ResponseHeadersRead, ct);

            if (resp.StatusCode != (HttpStatusCode)429)
                return resp;

            // 429: Too Many Requests
            resp.Dispose();

            int delayMs = 1000 * attempt; // fallback
            if (resp.Headers.RetryAfter?.Delta != null)
                delayMs = (int)resp.Headers.RetryAfter.Delta.Value.TotalMilliseconds;
            else if (resp.Headers.RetryAfter?.Date != null)
            {
                var d = resp.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                if (d.TotalMilliseconds > 0) delayMs = (int)d.TotalMilliseconds;
            }

            await Task.Delay(delayMs, ct);
        }

        // 最後にそのまま返してエラー内容を上位で見えるようにする
        using var last = Clone(req);
        return await http.SendAsync(last, HttpCompletionOption.ResponseHeadersRead, ct);
    }
}
