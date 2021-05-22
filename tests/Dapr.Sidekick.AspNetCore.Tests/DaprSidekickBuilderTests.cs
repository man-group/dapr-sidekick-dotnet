using System.Linq;
using Dapr.Sidekick.AspNetCore.Metrics;
using Dapr.Sidekick.AspNetCore.Placement;
using Dapr.Sidekick.AspNetCore.Sentry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore
{
    public class DaprSidekickBuilderTests
    {
        public class AddPlacement
        {
            [Test]
            public void Should_add_core_services()
            {
                var services = new ServiceCollection();
                var builder = services.AddDaprSidekick();
                Assert.That(builder, Is.TypeOf<DaprSidekickBuilder>());
                Assert.That(builder.AddPlacement(), Is.SameAs(builder));

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
        }

        public class AddSentry
        {
            [Test]
            public void Should_add_core_services()
            {
                var services = new ServiceCollection();
                var builder = services.AddDaprSidekick();
                Assert.That(builder, Is.TypeOf<DaprSidekickBuilder>());
                Assert.That(builder.AddSentry(), Is.SameAs(builder));

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
        }
    }
}
