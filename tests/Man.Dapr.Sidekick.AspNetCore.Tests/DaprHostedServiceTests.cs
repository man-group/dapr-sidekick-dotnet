using System;
using System.Threading;
using System.Threading.Tasks;
using Man.Dapr.Sidekick.Process;
using Man.Dapr.Sidekick.Threading;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore
{
    public class DaprHostedServiceTests
    {
        public class StartAsync
        {
            [Test]
            public async Task Should_start()
            {
                var options = new DaprOptions();
                var optionsAccessor = Substitute.For<IOptionsMonitor<DaprOptions>>();
                optionsAccessor.CurrentValue.Returns(options);
                var processHost = Substitute.For<IDaprProcessHost>();
                var host = new MockHostedService(processHost, optionsAccessor);
                var cancellationToken = CancellationToken.None;

                processHost.When(x => x.Start(Arg.Any<Func<DaprOptions>>(), Arg.Any<DaprCancellationToken>())).Do(ci =>
                {
                    // Check the accessor
                    var accessor = (Func<DaprOptions>)ci[0];
                    Assert.That(accessor(), Is.SameAs(options));
                    Assert.That(host.StartingOptions, Is.SameAs(options));
                    Assert.That(host.StartingCancellationToken, Is.EqualTo(cancellationToken));

                    // Check the cancellation token
                    var dct = (DaprCancellationToken)ci[1];
                    Assert.That(dct.CancellationToken, Is.EqualTo(cancellationToken));
                });

                await host.StartAsync(cancellationToken);
            }

            [Test]
            public async Task Should_stop()
            {
                var options = new DaprOptions();
                var optionsAccessor = Substitute.For<IOptionsMonitor<DaprOptions>>();
                var processHost = Substitute.For<IDaprProcessHost>();
                var host = new MockHostedService(processHost, optionsAccessor);
                var cancellationToken = CancellationToken.None;

                processHost.When(x => x.Stop(Arg.Any<DaprCancellationToken>())).Do(ci =>
                {
                    // Check the cancellation token
                    var dct = (DaprCancellationToken)ci[0];
                    Assert.That(dct.CancellationToken, Is.EqualTo(cancellationToken));
                });

                await host.StopAsync(cancellationToken);
            }
        }

        private class MockHostedService : DaprHostedService<IDaprProcessHost, MockDaprProcessOptions>
        {
            public MockHostedService(IDaprProcessHost daprProcessHost, IOptionsMonitor<DaprOptions> optionsAccessor)
                : base(daprProcessHost, optionsAccessor)
            {
            }

            public DaprOptions StartingOptions { get; private set; }

            public CancellationToken StartingCancellationToken { get; private set; }

            protected override void OnStarting(DaprOptions options, CancellationToken cancellationToken)
            {
                StartingOptions = options;
                StartingCancellationToken = cancellationToken;
                base.OnStarting(options, cancellationToken);
            }
        }
    }
}
