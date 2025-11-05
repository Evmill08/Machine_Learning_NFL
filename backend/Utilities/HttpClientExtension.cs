using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace backend.Utilities
{
    public static class HttpClientExtensions
    {
        private static readonly SemaphoreSlim _rateLimiter = new(3); 
        private static readonly TimeSpan _baseDelay = TimeSpan.FromMilliseconds(500);

        public static async Task<T?> GetFromJsonResilientAsync<T>(
            this HttpClient client,
            string url,
            int maxRetries = 5,
            JsonSerializerOptions? jsonOptions = null)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                await _rateLimiter.WaitAsync();
                try
                {
                    // Small random jitter to avoid synchronized bursts
                    await Task.Delay(Random.Shared.Next(400, 800));

                    var result = await client.GetFromJsonAsync<T>(url, jsonOptions);
                    return result;
                }
                catch (HttpRequestException ex) when (
                    ex.StatusCode == HttpStatusCode.Forbidden ||
                    ex.StatusCode == HttpStatusCode.TooManyRequests ||
                    ex.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    if (attempt == maxRetries)
                        throw;

                    int backoff = (int)(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt)) 
                                  + Random.Shared.Next(200);
                    Console.WriteLine($"[WARN] {ex.StatusCode} for {url}. Retrying in {backoff}ms...");
                    await Task.Delay(backoff);
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    int backoff = (int)(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt));
                    Console.WriteLine($"[WARN] Error: {ex.Message}. Retrying in {backoff}ms...");
                    await Task.Delay(backoff);
                }
                finally
                {
                    _rateLimiter.Release();
                }
            }

            return default;
        }
    }
}
