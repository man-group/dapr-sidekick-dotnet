using System.Linq;
using Man.Dapr.Sidekick.AspNetCore.Metrics;
using Man.Dapr.Sidekick.AspNetCore.Placement;
using Man.Dapr.Sidekick.AspNetCore.Sentry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Man.Dapr.Sidekick.AspNetCore
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

            // Override the default sidecar hosted service, to one that only starts when the Placement service is available.
            ReplaceSidecarHostedService<DaprPlacementSidecarHostedService>();

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
            ReplaceSidecarHostedService<DaprSentrySidecarHostedService>();

            return this;
        }

        private void ReplaceSidecarHostedService<TImplementation>()
            where TImplementation : class, IHostedService
        {
            var sidecarHostedServiceDescriptor = _services.FirstOrDefault(x => x.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) && x.ImplementationType == typeof(Sidecar.DaprSidecarHostedService));
            if (sidecarHostedServiceDescriptor != null)
            {
                _services.Remove(sidecarHostedServiceDescriptor);
                _services.AddHostedService<TImplementation>();
            }
        }
    }
}
