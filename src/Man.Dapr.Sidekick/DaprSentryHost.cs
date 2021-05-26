using Man.Dapr.Sidekick.Http;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Process;

namespace Man.Dapr.Sidekick
{
    public class DaprSentryHost : DaprProcessHost<DaprSentryOptions>, IDaprSentryHost
    {
        public DaprSentryHost(IDaprProcessFactory daprProcessFactory, IDaprProcessHttpClientFactory daprHttpClientFactory, IDaprLoggerFactory loggerFactory)
            : base(() => daprProcessFactory.CreateDaprSentryProcess(), daprHttpClientFactory, new DaprLogger<DaprSentryHost>(loggerFactory))
        {
        }
    }
}
