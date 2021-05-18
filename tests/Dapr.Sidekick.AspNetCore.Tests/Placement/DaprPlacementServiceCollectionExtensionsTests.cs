using System;
using System.Linq;
using Dapr.Sidekick.AspNetCore.Metrics;
using Dapr.Sidekick.Process;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Placement
{
    public class DaprPlacementServiceCollectionExtensionsTests
    {
        public class AddDaprPlacement
        {
            [Test]
            public void Should_add_core_services()
            {
                var services = new ServiceCollection().AddDaprSidecar(); // Sidecar services are required
                Assert.That(services.AddDaprPlacement(), Is.SameAs(services));

                var provider = services.BuildServiceProvider();
                var options = provider.GetRequiredService<IOptions<DaprOptions>>();
                Assert.That(options.Value, Is.Not.Null);

                // Check all services
                Assert.That(provider.GetRequiredService<IDaprPlacementHost>(), Is.InstanceOf<DaprPlacementHost>());
                Assert.That(provider.GetRequiredService<IPrometheusCollector>(), Is.InstanceOf<DaprPlacementMetricsCollector>());

                // Hosted Services
                var hostedServices = provider.GetServices<IHostedService>();
                Assert.That(hostedServices.Any(x => x.GetType() == typeof(DaprPlacementHostedService)), Is.True);
                Assert.That(hostedServices.Any(x => x.GetType() == typeof(DaprPlacementSidecarHostedService)), Is.True);
            }

            [Test]
            public void Should_add_core_services_with_configure_action()
            {
                var services = new ServiceCollection().AddDaprSidecar(); // Sidecar services are required
                Assert.That(
                    services.AddDaprPlacement(options =>
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
                    () => services.AddDaprPlacement(null, _ => { }));
            }

            [Test]
            public void Should_add_core_services_with_post_configure_action()
            {
                var configuration = Substitute.For<IConfiguration>();
                var services = new ServiceCollection().AddDaprSidecar(); // Sidecar services are required
                Assert.That(
                    services.AddDaprPlacement(configuration, options =>
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
