using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Sidekick.Threading;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Placement
{
    public class DaprPlacementHostedServiceTests
    {
        public class OnStarting
        {
            [Test]
            public async Task Should_set_options()
            {
                var options = new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions
                    {
                        AppId = "TEST_APP"
                    }
                };

                var optionsAccessor = Substitute.For<IOptionsMonitor<DaprOptions>>();
                optionsAccessor.CurrentValue.Returns(options);
                var host = Substitute.For<IDaprPlacementHost>();
                var hostedService = new DaprPlacementHostedService(host, optionsAccessor);
                var cancellationToken = CancellationToken.None;

                host.When(x => x.Start(Arg.Any<Func<DaprOptions>>(), Arg.Any<DaprCancellationToken>())).Do(ci =>
                {
                    // Execute the accessor
                    var accessor = (Func<DaprOptions>)ci[0];
                    var newOptions = accessor();
                    Assert.That(newOptions, Is.SameAs(options));

                    // Check the options have been updated
                    Assert.That(options.Placement, Is.Not.Null);
                    Assert.That(options.Placement.Metrics, Is.Not.Null);
                    Assert.That(options.Placement.Metrics.Labels["app"], Is.EqualTo("dapr-placement"));
                    Assert.That(options.Placement.Metrics.Labels["service"], Is.EqualTo("TEST_APP"));
                });

                await hostedService.StartAsync(cancellationToken);
            }
        }
    }
}
