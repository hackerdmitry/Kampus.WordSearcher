using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kampus.WordSearcher
{
    internal class HttpClientWithRetries : IDisposable 
    {
        public HttpClientWithRetries(int retries, TimeSpan tryTimeout, TimeSpan retryTimeout)
        {
            this.retries = retries;
            this.tryTimeout = tryTimeout;
            this.retryTimeout = retryTimeout;
            client = new HttpClient();
        }

        public HttpRequestHeaders DefaultHeaders => client.DefaultRequestHeaders;

        public Result<string> PostWithRetries(Uri uri, string data, string contentType)
        {
            return PostWithRetriesRaw(uri, data, contentType).Select(r => r.Content.ReadAsStringAsync().Result);
        }

        public Result<HttpResponseMessage> PostWithRetriesRaw(Uri uri, string data, string contentType)
        {
            var content = string.IsNullOrEmpty(data) ? null : new StringContent(data, Encoding.UTF8, contentType);
            return SendRequestWithRetries(ct => client.PostAsync(uri, content, ct)).Result;
        }

        public Result<string> GetWithRetries(Uri uri)
        {
            return SendRequestWithRetries(ct => client.GetAsync(uri, ct)).Result.Select(r => r.Content.ReadAsStringAsync().Result);
        }

        private async Task<Result<HttpResponseMessage>> SendRequestWithRetries(Func<CancellationToken, Task<HttpResponseMessage>> action)
        {
            for (var i = 0; i < retries; i++)
            {
                try
                {
                    var cts = new CancellationTokenSource(tryTimeout);
                    var result = (await action(cts.Token));
                    if (result.IsSuccessStatusCode)
                        return Result<HttpResponseMessage>.Success(result);
                    if (result.StatusCode == HttpStatusCode.Unauthorized)
                        return Result<HttpResponseMessage>.Fail(Status.Unauthorized);
                    if (result.StatusCode == HttpStatusCode.Forbidden)
                        return Result<HttpResponseMessage>.Fail(Status.Forbidden);
                    if (result.StatusCode == HttpStatusCode.Conflict)
                        return Result<HttpResponseMessage>.Fail(Status.Conflict);
                    if (result.StatusCode == (HttpStatusCode)429)
                        return Result<HttpResponseMessage>.Fail(Status.TooManyRequests);
                }
                catch (TaskCanceledException) { }
                Thread.Sleep(retryTimeout);
            }
            throw new AggregateException("Internal server error");
        }

        public void Dispose() => client.Dispose();

        private readonly HttpClient client;
        private readonly int retries;
        private readonly TimeSpan tryTimeout;
        private readonly TimeSpan retryTimeout;
    }
}