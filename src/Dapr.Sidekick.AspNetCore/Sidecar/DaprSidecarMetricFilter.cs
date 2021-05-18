using Dapr.Sidekick.AspNetCore.Metrics;

namespace Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarMetricFilter : IPrometheusMetricFilter
    {
        public bool ExcludeMetricLine(string name, string lineText)
        {
            // Exclude calls rejected due to API tokens/security
            return lineText.Contains("status=\"401\"");
        }
    }
}
