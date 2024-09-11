using System;
using System.Threading;
using System.Threading.Tasks;
using Man.Dapr.Sidekick.AspNetCore.Sidecar;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Man.Dapr.Sidekick.AspNetCore.Scheduler
{
    /// <summary>
    /// A hosted service for managing the sidecar lifetime.
    /// Specifically waits for the Scheduler service to start successfully and allocate ports/environment variables
    /// before starting itself.
    /// </summary>
    public class DaprSchedulerSidecarHostedService : DaprSidecarHostedService
    {
        private readonly IDaprSchedulerHost _daprSchedulerHost;
        private readonly ILogger<DaprSidecarHostedService> _logger;

        public DaprSchedulerSidecarHostedService(
            IDaprSidecarHost daprSidecarHost,
            IDaprSchedulerHost daprSchedulerHost,
            IOptionsMonitor<DaprOptions> optionsAccessor,
            ILogger<DaprSchedulerSidecarHostedService> logger,
            IServiceProvider serviceProvider = null)
            : base(daprSidecarHost, optionsAccessor, serviceProvider)
        {
            _daprSchedulerHost = daprSchedulerHost;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(
                async () =>
                {
                    var processInfo = _daprSchedulerHost.GetProcessInfo();
                    while (!processInfo.IsRunning && processInfo.Status != Process.DaprProcessStatus.Disabled)
                    {
                        _logger.LogInformation("Dapr Sidecar process is waiting for the Dapr Scheduler process to finish starting up...");
                        await Task.Delay(250);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        processInfo = _daprSchedulerHost.GetProcessInfo();
                    }
                },
                cancellationToken)
                .ContinueWith(_ => base.StartAsync(cancellationToken));
        }
    }
}
