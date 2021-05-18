using Dapr.Sidekick.AspNetCore.Metrics;

namespace Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarMetricsCollector : DaprProcessHostPrometheusCollector
    {
        public DaprSidecarMetricsCollector(IDaprSidecarHost daprSidecarHost)
            : base(daprSidecarHost)
        {
        }
    }
}
