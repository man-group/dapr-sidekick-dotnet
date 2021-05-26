#if NET35
using System.Net;

namespace Man.Dapr.Sidekick.Http
{
    internal partial class DaprProcessHttpClientFactory
    {
        public WebClient CreateDaprWebClient()
        {
            EnsureNotDisposed();

            return new WebClient();
        }
    }
}
#endif
