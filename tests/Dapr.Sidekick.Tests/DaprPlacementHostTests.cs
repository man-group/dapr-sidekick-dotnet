using System;
using Dapr.Sidekick.Http;
using Dapr.Sidekick.Logging;
using Dapr.Sidekick.Process;
using Dapr.Sidekick.Threading;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick
{
    public class DaprPlacementHostTests
    {
        public class Constructor
        {
            [Test]
            public void Should_set_properties()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprPlacementProcess = Substitute.For<IDaprPlacementProcess>();
                daprProcessFactory.CreateDaprPlacementProcess().Returns(daprPlacementProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();

                var host = new DaprPlacementHost(daprProcessFactory, daprHttpClientFactory, loggerFactory);

                // Check HttpClientFactory
                Assert.That(host.DaprHttpClientFactory, Is.SameAs(daprHttpClientFactory));

                // Check Logger
                Assert.That(host.Logger, Is.InstanceOf<DaprLogger<DaprPlacementHost>>());
                loggerFactory.Received(1).CreateLogger("Dapr.Sidekick.DaprPlacementHost");

                // Check ProcessFactory
                var cancellationToken = DaprCancellationToken.None;
                host.Start(null, cancellationToken);
                daprPlacementProcess.Received(1).Start(Arg.Any<Func<DaprOptions>>(), host.Logger, cancellationToken);
            }
        }
    }
}
