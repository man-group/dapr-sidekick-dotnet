using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Sidekick.AspNetCore.Sidecar;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dapr.Sidekick.AspNetCore.Placement
{
    /// <summary>
    /// A hosted service for managing the sidecar lifetime.
    /// Specifically waits for the placement service to start successfully and allocate ports/environment variables
    /// before starting itself.
    /// </summary>
    public class DaprPlacementSidecarHostedService : DaprSidecarHostedService
    {
        private readonly IDaprPlacementHost _daprPlacementHost;
        private readonly ILogger<DaprSidecarHostedService> _logger;

        public DaprPlacementSidecarHostedService(
            IDaprSidecarHost daprSidecarHost,
            IDaprPlacementHost daprPlacementHost,
            IOptionsMonitor<DaprOptions> optionsAccessor,
            ILogger<DaprPlacementSidecarHostedService> logger,
            IServiceProvider serviceProvider = null)
            : base(daprSidecarHost, optionsAccessor, serviceProvider)
        {
            _daprPlacementHost = daprPlacementHost;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(
                async () =>
                {
                    while (!_daprPlacementHost.GetProcessInfo().IsRunning)
                    {
                        _logger.LogInformation("Dapr Sidecar process is waiting for the Dapr Placement process to finish starting up...");
                        await Task.Delay(250);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }, cancellationToken)
                .ContinueWith(_ => base.StartAsync(cancellationToken));
        }
    }
}
