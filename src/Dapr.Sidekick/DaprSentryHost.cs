using Dapr.Sidekick.Http;
using Dapr.Sidekick.Logging;
using Dapr.Sidekick.Process;

namespace Dapr.Sidekick
{
    public class DaprSentryHost : DaprProcessHost<DaprSentryOptions>, IDaprSentryHost
    {
        public DaprSentryHost(IDaprProcessFactory daprProcessFactory, IDaprProcessHttpClientFactory daprHttpClientFactory, IDaprLoggerFactory loggerFactory)
            : base(() => daprProcessFactory.CreateDaprSentryProcess(), daprHttpClientFactory, new DaprLogger<DaprSentryHost>(loggerFactory))
        {
        }
    }
}
