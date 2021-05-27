#if !NET35
using System.Net.Http;

namespace Man.Dapr.Sidekick.Http
{
    internal partial class DaprProcessHttpClientFactory
    {
        private HttpClient _httpClient;

        public DaprProcessHttpClientFactory()
        {
            _httpClient = new HttpClient();
        }

        public DaprProcessHttpClientFactory(HttpClientHandler handler)
        {
            _httpClient = new HttpClient(handler);
        }

        public HttpClient CreateDaprHttpClient()
        {
            EnsureNotDisposed();

            return _httpClient;
        }

        protected override void OnDisposing(bool disposing)
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }

            base.OnDisposing(disposing);
        }
    }
}
#endif
