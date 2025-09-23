using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace msitemap
{
    public record LinkCheckResult(
        string Url,
        HttpStatusCode? Status,
        bool IsRedirect,
        string? RedirectLocation,
        bool IsBroken404,
        string? Error,
        long ElapsedMs
    );

    public static class LinkChecker
    {
        // Основной метод: проверяет список ссылок параллельно
        public static async Task<List<LinkCheckResult>> CheckAsync(
            IEnumerable<string> urls,
            int degreeOfParallelism = 12,
            TimeSpan? timeout = null,
            CancellationToken ct = default)
        {
            // Клиент без авторедиректа — чтобы увидеть сам код редиректа
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using var client = new HttpClient(handler)
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(12)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("LinkChecker/1.0 (+https://example.com)");

            var results = new List<LinkCheckResult>();
            var semaphore = new SemaphoreSlim(degreeOfParallelism);

            var tasks = urls.Select(async url =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    var res = await CheckOneAsync(client, url, ct);
                    lock (results) results.Add(res);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            return results;
        }

        // Проверка одной ссылки: HEAD -> (при необходимости) GET
        private static async Task<LinkCheckResult> CheckOneAsync(HttpClient client, string url, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                using var headReq = new HttpRequestMessage(HttpMethod.Head, url);
                using var headResp = await client.SendAsync(headReq, HttpCompletionOption.ResponseHeadersRead, ct);

                // Если сайт не поддерживает HEAD — пробуем GET
                if (headResp.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotImplemented or HttpStatusCode.Forbidden)
                {
                    using var getReq = new HttpRequestMessage(HttpMethod.Get, url);
                    using var getResp = await client.SendAsync(getReq, HttpCompletionOption.ResponseHeadersRead, ct);
                    return BuildResult(url, getResp, sw);
                }

                return BuildResult(url, headResp, sw);
            }
            catch (TaskCanceledException ex)
            {
                sw.Stop();
                // Отличаем таймаут от внешней отмены
                var msg = ex.CancellationToken == ct ? "Canceled" : "Timeout";
                return new LinkCheckResult(url, null, false, null, false, msg, sw.ElapsedMilliseconds);
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                return new LinkCheckResult(url, null, false, null, false, ex.Message, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new LinkCheckResult(url, null, false, null, false, ex.Message, sw.ElapsedMilliseconds);
            }
        }

        private static LinkCheckResult BuildResult(string url, HttpResponseMessage resp, Stopwatch sw)
        {
            sw.Stop();

            var code = resp.StatusCode;
            var isRedirect = (int)code is 301 or 302 or 307 or 308;
            var location = isRedirect
                ? resp.Headers.Location?.ToString()
                : null;

            var isBroken404 = code == HttpStatusCode.NotFound;

            return new LinkCheckResult(
                Url: url,
                Status: code,
                IsRedirect: isRedirect,
                RedirectLocation: location,
                IsBroken404: isBroken404,
                Error: null,
                ElapsedMs: sw.ElapsedMilliseconds
            );
        }
    }
}
