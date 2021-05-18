using Dapr.Sidekick.Http;
using Dapr.Sidekick.Logging;
using Dapr.Sidekick.Process;

namespace Dapr.Sidekick
{
    public sealed class DaprPlacementHost : DaprProcessHost<DaprPlacementOptions>, IDaprPlacementHost
    {
        public DaprPlacementHost(IDaprProcessFactory daprProcessFactory, IDaprProcessHttpClientFactory daprHttpClientFactory, IDaprLoggerFactory loggerFactory)
            : base(() => daprProcessFactory.CreateDaprPlacementProcess(), daprHttpClientFactory, new DaprLogger<DaprPlacementHost>(loggerFactory))
        {
        }
    }
}
