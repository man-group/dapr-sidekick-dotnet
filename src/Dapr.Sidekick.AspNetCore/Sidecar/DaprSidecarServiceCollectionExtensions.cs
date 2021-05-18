using System;
using Dapr.Sidekick;
using Dapr.Sidekick.AspNetCore.Metrics;
using Dapr.Sidekick.AspNetCore.Sidecar;
using Dapr.Sidekick.Http;
using Dapr.Sidekick.Logging;
using Dapr.Sidekick.Process;
using Dapr.Sidekick.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DaprSidecarServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Dapr Sidecar process to the service container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureAction">An optional action to configure the component.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddDaprSidecar(this IServiceCollection services, Action<DaprOptions> configureAction = null)
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
        /// Adds the Dapr Sidecar process to the service container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">A <see cref="IConfiguration"/> instance for configuring the component.</param>
        /// <param name="postConfigureAction">An optional action to configure the component after initial configuration is applied.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddDaprSidecar(this IServiceCollection services, IConfiguration configuration, Action<DaprOptions> postConfigureAction = null) =>
            AddDaprSidecar(services, DaprOptions.SectionName, configuration, postConfigureAction);

        /// <summary>
        /// Adds the Dapr Sidecar process to the service container using the configuration section specified by <paramref name="name"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="name">The name of the configuration section containing the settings in <paramref name="configuration"/>.</param>
        /// <param name="configuration">A <see cref="IConfiguration"/> instance for configuring the component.</param>
        /// <param name="postConfigureAction">An optional action to configure the component after initial configuration is applied.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddDaprSidecar(this IServiceCollection services, string name, IConfiguration configuration, Action<DaprOptions> postConfigureAction = null)
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

            // The Dapr Sidecar can take 5 seconds to shutdown, which is the default shutdown time for IHostedService.
            // So set the default shutdown timeout to 10 seconds. This can be overridden in configuration using HostOptions.
            // See https://andrewlock.net/extending-the-shutdown-timeout-setting-to-ensure-graceful-ihostedservice-shutdown/
            services.Configure<HostOptions>(opts => opts.ShutdownTimeout = System.TimeSpan.FromSeconds(10));

            return services;
        }

        private static void AddCoreServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging().TryAddSingleton<IDaprLoggerFactory, Dapr.Sidekick.Extensions.Logging.DaprLoggerFactory>();

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IDaprProcessFactory, DaprProcessFactory>();
            services.TryAddSingleton<IDaprSidecarHost, DaprSidecarHost>();
            services.TryAddSingleton<DaprHttpContextHttpClientFactory>();
            services.TryAddSingleton<IDaprProcessHttpClientFactory>(x => x.GetRequiredService<DaprHttpContextHttpClientFactory>());
            services.TryAddSingleton<IDaprSidecarHttpClientFactory>(x => x.GetRequiredService<DaprHttpContextHttpClientFactory>());
            services.TryAddSingleton<IDaprApiTokenProvider, RandomStringApiTokenProvider>();
            services.TryAddSingleton<DaprApiTokenManager>();
            services.TryAddSingleton<IDaprApiTokenAccessor>(x => x.GetRequiredService<DaprApiTokenManager>());
            services.TryAddSingleton<IDaprApiTokenManager>(x => x.GetRequiredService<DaprApiTokenManager>());
            // If the service collection does not already contain a DaprSidecarHostedService implementation, don't try to add another one
            services.TryAddHostedService<DaprSidecarHostedService>();

            // Add the health checks and metrics
            services.AddHealthChecks().AddDaprSidecar();
            services.AddSingleton<IPrometheusCollector, DaprSidecarMetricsCollector>();
            services.AddSingleton<IPrometheusMetricFilter, DaprSidecarMetricFilter>();
            services.TryAddSingleton<IDaprMetricsCollectorRegistry, PrometheusCollectorRegistry>();
        }
    }
}
