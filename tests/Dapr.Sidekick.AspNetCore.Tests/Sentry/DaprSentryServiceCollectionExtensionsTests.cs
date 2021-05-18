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

namespace Dapr.Sidekick.AspNetCore.Sentry
{
    public class DaprSentryServiceCollectionExtensionsTests
    {
        public class AddDaprSentry
        {
            [Test]
            public void Should_add_core_services()
            {
                var services = new ServiceCollection().AddDaprSidecar(); // Sidecar services are required
                Assert.That(services.AddDaprSentry(), Is.SameAs(services));

                var provider = services.BuildServiceProvider();
                var options = provider.GetRequiredService<IOptions<DaprOptions>>();
                Assert.That(options.Value, Is.Not.Null);

                // Check all services
                Assert.That(provider.GetRequiredService<IDaprSentryHost>(), Is.InstanceOf<DaprSentryHost>());
                Assert.That(provider.GetRequiredService<IPrometheusCollector>(), Is.InstanceOf<DaprSentryMetricsCollector>());

                // Hosted Services
                var hostedServices = provider.GetServices<IHostedService>();
                Assert.That(hostedServices.Any(x => x.GetType() == typeof(DaprSentryHostedService)), Is.True);
                Assert.That(hostedServices.Any(x => x.GetType() == typeof(DaprSentrySidecarHostedService)), Is.True);
            }

            [Test]
            public void Should_add_core_services_with_configure_action()
            {
                var services = new ServiceCollection().AddDaprSidecar(); // Sidecar services are required
                Assert.That(
                    services.AddDaprSentry(options =>
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
                    () => services.AddDaprSentry(null, _ => { }));
            }

            [Test]
            public void Should_add_core_services_with_post_configure_action()
            {
                var configuration = Substitute.For<IConfiguration>();
                var services = new ServiceCollection().AddDaprSidecar(); // Sidecar services are required
                Assert.That(
                    services.AddDaprSentry(configuration, options =>
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
