using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore
{
    public class DaprServiceCollectionExtensionsTests
    {
        public class TryAddHostedService
        {
            [Test]
            public void Should_add_when_not_exists()
            {
                var services = new ServiceCollection();

                // First add
                services.TryAddHostedService<Sentry.DaprSentryHostedService>();
                Assert.That(services.Count, Is.EqualTo(1));

                // Second add
                services.TryAddHostedService<Sentry.DaprSentryHostedService>();
                Assert.That(services.Count, Is.EqualTo(1));
            }
        }

        public class HasAssignableService
        {
            [Test]
            public void Should_return_false_when_no_service_type_match()
            {
                var services = new ServiceCollection();
                services.AddTransient<DaprHostedServiceTests>();
                services.AddTransient<DaprProcessHealthCheckTests>();

                Assert.That(services.HasAssignableService<IHostedService, Sentry.DaprSentryHostedService>(), Is.False);
            }

            [Test]
            public void Should_return_false_when_no_implementation_type_match()
            {
                var services = new ServiceCollection();
                services.AddTransient<DaprHostedServiceTests>();
                services.AddTransient<IHostedService, Placement.DaprPlacementHostedService>();

                Assert.That(services.HasAssignableService<IHostedService, Sentry.DaprSentryHostedService>(), Is.False);
            }

            [Test]
            public void Should_return_true()
            {
                var services = new ServiceCollection();
                services.AddTransient<IHostedService, Sentry.DaprSentryHostedService>();

                Assert.That(services.HasAssignableService<IHostedService, Sentry.DaprSentryHostedService>(), Is.True);
            }
        }
    }
}
