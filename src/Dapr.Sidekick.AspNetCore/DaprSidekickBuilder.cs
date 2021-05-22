using Dapr.Sidekick.AspNetCore.Metrics;
using Dapr.Sidekick.AspNetCore.Placement;
using Dapr.Sidekick.AspNetCore.Sentry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dapr.Sidekick.AspNetCore
{
    internal class DaprSidekickBuilder : IDaprSidekickBuilder
    {
        private readonly IServiceCollection _services;

        internal DaprSidekickBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IDaprSidekickBuilder AddPlacement()
        {
            // Add the placement host
            _services.TryAddSingleton<IDaprPlacementHost, DaprPlacementHost>();
            _services.TryAddHostedService<DaprPlacementHostedService>();

            // Add the health checks and metrics
            _services.AddHealthChecks().AddDaprPlacement();
            _services.AddSingleton<IPrometheusCollector, DaprPlacementMetricsCollector>();

            // Override the default sidecar hosted service, to one that only starts when the placement service is available.
            _services.TryAddHostedService<DaprPlacementSidecarHostedService>();

            return this;
        }

        public IDaprSidekickBuilder AddSentry()
        {
            // Add the Sentry host
            _services.TryAddSingleton<IDaprSentryHost, DaprSentryHost>();
            _services.TryAddHostedService<DaprSentryHostedService>();

            // Add the health checks and metrics
            _services.AddHealthChecks().AddDaprSentry();
            _services.AddSingleton<IPrometheusCollector, DaprSentryMetricsCollector>();

            // Override the default sidecar hosted service, to one that only starts when the Sentry service is available.
            _services.TryAddHostedService<DaprSentrySidecarHostedService>();

            return this;
        }
    }
}
