using System;
using System.Linq;
using Dapr.Sidekick.AspNetCore.Metrics;
using Dapr.Sidekick.Http;
using Dapr.Sidekick.Process;
using Dapr.Sidekick.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarServiceCollectionExtensionsTests
    {
        public class AddDaprSidecar
        {
            [Test]
            public void Should_add_core_services()
            {
                var services = new ServiceCollection();
                Assert.That(services.AddDaprSidecar(), Is.SameAs(services));

                var provider = services.BuildServiceProvider();
                var options = provider.GetRequiredService<IOptions<DaprOptions>>();
                Assert.That(options.Value, Is.Not.Null);

                // Check all services
                Assert.That(provider.GetRequiredService<IHttpContextAccessor>(), Is.Not.Null);
                Assert.That(provider.GetRequiredService<IDaprProcessFactory>(), Is.InstanceOf<DaprProcessFactory>());
                Assert.That(provider.GetRequiredService<IDaprSidecarHost>(), Is.InstanceOf<DaprSidecarHost>());
                Assert.That(provider.GetRequiredService<IDaprProcessHttpClientFactory>(), Is.InstanceOf<DaprHttpContextHttpClientFactory>());
                Assert.That(provider.GetRequiredService<IDaprSidecarHttpClientFactory>(), Is.InstanceOf<DaprHttpContextHttpClientFactory>());
                Assert.That(provider.GetRequiredService<IDaprApiTokenProvider>(), Is.InstanceOf<RandomStringApiTokenProvider>());
                Assert.That(provider.GetRequiredService<IDaprApiTokenAccessor>(), Is.InstanceOf<DaprApiTokenManager>());
                Assert.That(provider.GetRequiredService<IDaprApiTokenManager>(), Is.InstanceOf<DaprApiTokenManager>());
                Assert.That(provider.GetRequiredService<IPrometheusCollector>(), Is.InstanceOf<DaprSidecarMetricsCollector>());
                Assert.That(provider.GetRequiredService<IPrometheusMetricFilter>(), Is.InstanceOf<DaprSidecarMetricFilter>());
                Assert.That(provider.GetRequiredService<IDaprMetricsCollectorRegistry>(), Is.InstanceOf<PrometheusCollectorRegistry>());

                // Hosted Services
                var hostedServices = provider.GetServices<IHostedService>();
                Assert.That(hostedServices.Any(x => x.GetType() == typeof(DaprSidecarHostedService)), Is.True);
                Assert.That(hostedServices.Any(x => x.GetType().Name == "HealthCheckPublisherHostedService"), Is.True);
            }

            [Test]
            public void Should_add_core_services_with_configure_action()
            {
                var services = new ServiceCollection();
                Assert.That(
                    services.AddDaprSidecar(options =>
                    {
                        options.ProcessName = "PROCESS_NAME";
                    }), Is.SameAs(services));

                var provider = services.BuildServiceProvider();
                var options = provider.GetRequiredService<IOptions<DaprOptions>>();
                Assert.That(options.Value.ProcessName, Is.EqualTo("PROCESS_NAME"));
                Assert.That(provider.GetRequiredService<IDaprProcessFactory>(), Is.Not.Null);
            }

            [Test]
            public void Should_throw_exception_when_null_configuration()
            {
                var services = new ServiceCollection();
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("configuration"),
                    () => services.AddDaprSidecar(null, _ => { }));
            }

            [Test]
            public void Should_add_core_services_with_post_configure_action()
            {
                var configuration = Substitute.For<IConfiguration>();
                var services = new ServiceCollection();
                Assert.That(
                    services.AddDaprSidecar(configuration, options =>
                    {
                        options.ProcessName = "PROCESS_NAME";
                    }), Is.SameAs(services));

                var provider = services.BuildServiceProvider();
                var options = provider.GetRequiredService<IOptions<DaprOptions>>();
                Assert.That(options.Value.ProcessName, Is.EqualTo("PROCESS_NAME"));
                Assert.That(provider.GetRequiredService<IDaprProcessFactory>(), Is.Not.Null);
                configuration.Received(1).GetSection("Dapr");
            }
        }
    }
}
