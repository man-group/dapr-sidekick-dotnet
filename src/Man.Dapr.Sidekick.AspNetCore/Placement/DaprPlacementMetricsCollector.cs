using Man.Dapr.Sidekick.AspNetCore.Metrics;

namespace Man.Dapr.Sidekick.AspNetCore.Placement
{
    public class DaprPlacementMetricsCollector : DaprProcessHostPrometheusCollector
    {
        public DaprPlacementMetricsCollector(IDaprPlacementHost daprPlacementHost)
            : base(daprPlacementHost)
        {
        }
    }
}
