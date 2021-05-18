using Dapr.Sidekick.AspNetCore.Metrics;

namespace Dapr.Sidekick.AspNetCore.Sentry
{
    public class DaprSentryMetricsCollector : DaprProcessHostPrometheusCollector
    {
        public DaprSentryMetricsCollector(IDaprSentryHost daprSentryHost)
            : base(daprSentryHost)
        {
        }
    }
}
