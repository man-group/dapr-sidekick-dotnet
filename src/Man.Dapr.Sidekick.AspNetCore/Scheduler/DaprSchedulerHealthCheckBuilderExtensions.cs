using System.Collections.Generic;
using Man.Dapr.Sidekick.AspNetCore.Metrics;
using Man.Dapr.Sidekick.AspNetCore.Scheduler;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DaprSchedulerHealthCheckBuilderExtensions
    {
        /// <summary>
        /// Add a health check for the Dapr Scheduler.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'dapr_Scheduler' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/> to allow calls to be chained.</returns>
        public static IHealthChecksBuilder AddDaprScheduler(
            this IHealthChecksBuilder builder,
            string name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            builder.AddCheck<DaprSchedulerHealthCheck>(name ?? DaprMetricsConstants.DaprSchedulerLabel, failureStatus, tags);
            return builder;
        }
    }
}
