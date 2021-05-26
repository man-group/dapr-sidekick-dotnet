using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Sidekick.Process;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Dapr.Sidekick
{
    public abstract class DaprProcessHealthCheck : IHealthCheck
    {
        private readonly IDaprProcessHost _daprProcessHost;

        protected DaprProcessHealthCheck(IDaprProcessHost daprProcessHost)
        {
            _daprProcessHost = daprProcessHost;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Make sure the process is enabled
                var processInfo = _daprProcessHost.GetProcessInfo();
                if (processInfo.Status != DaprProcessStatus.Disabled)
                {
                    // Check to see if the process is running
                    if (!processInfo.IsRunning)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: processInfo.Description);
                    }

                    // Check the endpoint
                    var result = await _daprProcessHost.GetHealthAsync(cancellationToken);
                    if (!result.IsHealthy)
                    {
                        // Not Healthy
                        return new HealthCheckResult(context.Registration.FailureStatus, "Dapr process health check endpoint reports Unhealthy (" + result.StatusCode + ")");
                    }
                }

                return HealthCheckResult.Healthy(description: processInfo.Description);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context?.Registration?.FailureStatus ?? HealthStatus.Unhealthy, exception: ex);
            }
        }
    }
}
