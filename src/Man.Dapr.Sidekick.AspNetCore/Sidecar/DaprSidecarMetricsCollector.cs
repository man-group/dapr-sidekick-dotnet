using Man.Dapr.Sidekick.AspNetCore.Metrics;

namespace Man.Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarMetricsCollector : DaprProcessHostPrometheusCollector
    {
        public DaprSidecarMetricsCollector(IDaprSidecarHost daprSidecarHost)
            : base(daprSidecarHost)
        {
        }
    }
}
