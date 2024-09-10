using Man.Dapr.Sidekick.Http;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Process;

namespace Man.Dapr.Sidekick
{
    public sealed class DaprSchedulerHost : DaprProcessHost<DaprSchedulerOptions>, IDaprSchedulerHost
    {
        public DaprSchedulerHost(IDaprProcessFactory daprProcessFactory, IDaprProcessHttpClientFactory daprHttpClientFactory, IDaprLoggerFactory loggerFactory)
            : base(() => daprProcessFactory.CreateDaprSchedulerProcess(), daprHttpClientFactory, new DaprLogger<DaprSchedulerHost>(loggerFactory))
        {
        }
    }
}
