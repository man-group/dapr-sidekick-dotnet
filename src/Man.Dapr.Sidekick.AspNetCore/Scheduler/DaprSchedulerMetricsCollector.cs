using Man.Dapr.Sidekick.AspNetCore.Metrics;

namespace Man.Dapr.Sidekick.AspNetCore.Scheduler
{
    public class DaprSchedulerMetricsCollector : DaprProcessHostPrometheusCollector
    {
        public DaprSchedulerMetricsCollector(IDaprSchedulerHost daprSchedulerHost)
            : base(daprSchedulerHost)
        {
        }
    }
}
