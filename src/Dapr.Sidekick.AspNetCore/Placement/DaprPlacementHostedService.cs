using System.Threading;
using Dapr.Sidekick.AspNetCore.Metrics;
using Microsoft.Extensions.Options;

namespace Dapr.Sidekick.AspNetCore.Placement
{
    public class DaprPlacementHostedService : DaprHostedService<IDaprPlacementHost, DaprPlacementOptions>
    {
        public DaprPlacementHostedService(
            IDaprPlacementHost daprPlacementHost,
            IOptionsMonitor<DaprOptions> optionsAccessor)
            : base(daprPlacementHost, optionsAccessor)
        {
        }

        protected override void OnStarting(DaprOptions options, CancellationToken cancellationToken)
        {
            // Assign metrics
            options.Placement ??= new DaprPlacementOptions();
            options.Placement.Metrics ??= new DaprMetricsOptions();
            options.Placement.Metrics.SetLabel(DaprMetricsConstants.ServiceLabelName, options.Sidecar?.AppId);
            options.Placement.Metrics.SetLabel(DaprMetricsConstants.AppLabelName, DaprMetricsConstants.DaprPlacementLabel);
        }
    }
}
