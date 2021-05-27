using Man.Dapr.Sidekick.AspNetCore.Metrics;

namespace Man.Dapr.Sidekick.AspNetCore.Sentry
{
    public class DaprSentryMetricsCollector : DaprProcessHostPrometheusCollector
    {
        public DaprSentryMetricsCollector(IDaprSentryHost daprSentryHost)
            : base(daprSentryHost)
        {
        }
    }
}
