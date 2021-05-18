﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Sidekick.AspNetCore.Sidecar;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dapr.Sidekick.AspNetCore.Sentry
{
    /// <summary>
    /// A hosted service for managing the sidecar lifetime.
    /// Specifically waits for the Sentry service to start successfully and allocate ports/environment variables
    /// before starting itself.
    /// </summary>
    public class DaprSentrySidecarHostedService : DaprSidecarHostedService
    {
        private readonly IDaprSentryHost _daprSentryHost;
        private readonly ILogger<DaprSidecarHostedService> _logger;

        public DaprSentrySidecarHostedService(
            IDaprSidecarHost daprSidecarHost,
            IDaprSentryHost daprSentryHost,
            IOptionsMonitor<DaprOptions> optionsAccessor,
            ILogger<DaprSentrySidecarHostedService> logger,
            IServiceProvider serviceProvider = null)
            : base(daprSidecarHost, optionsAccessor, serviceProvider)
        {
            _daprSentryHost = daprSentryHost;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(
                async () =>
                {
                    while (!_daprSentryHost.GetProcessInfo().IsRunning)
                    {
                        _logger.LogInformation("Dapr Sidecar process is waiting for the Dapr Sentry process to finish starting up...");
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
