using Dapr.Sidekick.AspNetCore.Metrics;

namespace Dapr.Sidekick.AspNetCore.Placement
{
    public class DaprPlacementMetricsCollector : DaprProcessHostPrometheusCollector
    {
        public DaprPlacementMetricsCollector(IDaprPlacementHost daprPlacementHost)
            : base(daprPlacementHost)
        {
        }
    }
}
