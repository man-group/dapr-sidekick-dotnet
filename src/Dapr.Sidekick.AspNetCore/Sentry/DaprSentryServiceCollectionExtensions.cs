using System;
using Dapr.Sidekick;
using Dapr.Sidekick.AspNetCore.Metrics;
using Dapr.Sidekick.AspNetCore.Sentry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DaprSentryServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Dapr Sentry process to the service container.
        /// Requires <see cref="DaprSidecarServiceCollectionExtensions.AddDaprSidecar(IServiceCollection, Action{DaprOptions})"/> to also be specified.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureAction">An optional action to configure the component.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddDaprSentry(this IServiceCollection services, Action<DaprOptions> configureAction = null)
        {
            AddCoreServices(services);

            // Configure the options
            if (configureAction != null)
            {
                services.AddOptions<DaprOptions>().Configure(configureAction);
            }
            else
            {
                services.AddOptions<DaprOptions>();
            }

            return services;
        }

        /// <summary>
        /// Adds the Dapr Sentry process to the service container.
        /// Requires <see cref="DaprSidecarServiceCollectionExtensions.AddDaprSidecar(IServiceCollection, Action{DaprOptions})"/> to also be specified.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">A <see cref="IConfiguration"/> instance for configuring the component.</param>
        /// <param name="postConfigureAction">An optional action to configure the component after initial configuration is applied.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddDaprSentry(this IServiceCollection services, IConfiguration configuration, Action<DaprOptions> postConfigureAction = null) =>
            AddDaprSentry(services, DaprOptions.SectionName, configuration, postConfigureAction);

        /// <summary>
        /// Adds the Dapr Sentry process to the service container using the configuration section specified by <paramref name="name"/>.
        /// Requires <see cref="DaprSidecarServiceCollectionExtensions.AddDaprSidecar(IServiceCollection, Action{DaprOptions})"/> to also be specified.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="name">The name of the configuration section containing the settings in <paramref name="configuration"/>.</param>
        /// <param name="configuration">A <see cref="IConfiguration"/> instance for configuring the component.</param>
        /// <param name="postConfigureAction">An optional action to configure the component after initial configuration is applied.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddDaprSentry(this IServiceCollection services, string name, IConfiguration configuration, Action<DaprOptions> postConfigureAction = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            AddCoreServices(services);

            services.Configure<DaprOptions>(configuration.GetSection(name));

            if (postConfigureAction != null)
            {
                services.PostConfigure(postConfigureAction);
            }

            return services;
        }

        private static void AddCoreServices(IServiceCollection services)
        {
            // Add the Sentry host
            services.TryAddSingleton<IDaprSentryHost, DaprSentryHost>();
            services.TryAddHostedService<DaprSentryHostedService>();

            // Add the health checks and metrics
            services.AddHealthChecks().AddDaprSentry();
            services.AddSingleton<IPrometheusCollector, DaprSentryMetricsCollector>();

            // Override the default sidecar hosted service, to one that only starts when the Sentry service is available.
            services.TryAddHostedService<DaprSentrySidecarHostedService>();
        }
    }
}
