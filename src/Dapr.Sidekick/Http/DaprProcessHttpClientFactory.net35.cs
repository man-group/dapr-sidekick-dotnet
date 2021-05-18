#if NET35
using System.Net;

namespace Dapr.Sidekick.Http
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
