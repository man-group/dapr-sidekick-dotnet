using Man.Dapr.Sidekick.Http;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Process;

namespace Man.Dapr.Sidekick
{
    public sealed class DaprPlacementHost : DaprProcessHost<DaprPlacementOptions>, IDaprPlacementHost
    {
        public DaprPlacementHost(IDaprProcessFactory daprProcessFactory, IDaprProcessHttpClientFactory daprHttpClientFactory, IDaprLoggerFactory loggerFactory)
            : base(() => daprProcessFactory.CreateDaprPlacementProcess(), daprHttpClientFactory, new DaprLogger<DaprPlacementHost>(loggerFactory))
        {
        }
    }
}
