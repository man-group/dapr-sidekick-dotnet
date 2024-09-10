using System.Threading;
using Man.Dapr.Sidekick.AspNetCore.Metrics;
using Microsoft.Extensions.Options;

namespace Man.Dapr.Sidekick.AspNetCore.Scheduler
{
    public class DaprSchedulerHostedService : DaprHostedService<IDaprSchedulerHost, DaprSchedulerOptions>
    {
        public DaprSchedulerHostedService(
            IDaprSchedulerHost daprSchedulerHost,
            IOptionsMonitor<DaprOptions> optionsAccessor)
            : base(daprSchedulerHost, optionsAccessor)
        {
        }

        protected override void OnStarting(DaprOptions options, CancellationToken cancellationToken)
        {
            // Assign metrics
            options.Scheduler ??= new DaprSchedulerOptions();
            options.Scheduler.Metrics ??= new DaprMetricsOptions();
            options.Scheduler.Metrics.SetLabel(DaprMetricsConstants.ServiceLabelName, options.Sidecar?.AppId);
            options.Scheduler.Metrics.SetLabel(DaprMetricsConstants.AppLabelName, DaprMetricsConstants.DaprSchedulerLabel);
        }
    }
}
