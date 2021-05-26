using System;
using Man.Dapr.Sidekick.Http;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Process;
using Man.Dapr.Sidekick.Threading;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick
{
    public class DaprSentryHostTests
    {
        public class Constructor
        {
            [Test]
            public void Should_set_properties()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprSentryProcess = Substitute.For<IDaprSentryProcess>();
                daprProcessFactory.CreateDaprSentryProcess().Returns(daprSentryProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();

                var host = new DaprSentryHost(daprProcessFactory, daprHttpClientFactory, loggerFactory);

                // Check HttpClientFactory
                Assert.That(host.DaprHttpClientFactory, Is.SameAs(daprHttpClientFactory));

                // Check Logger
                Assert.That(host.Logger, Is.InstanceOf<DaprLogger<DaprSentryHost>>());
                loggerFactory.Received(1).CreateLogger("Man.Dapr.Sidekick.DaprSentryHost");

                // Check ProcessFactory
                var cancellationToken = DaprCancellationToken.None;
                host.Start(null, cancellationToken);
                daprSentryProcess.Received(1).Start(Arg.Any<Func<DaprOptions>>(), host.Logger, cancellationToken);
            }
        }
    }
}
