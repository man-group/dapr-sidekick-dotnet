using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore.Sentry
{
    public class DaprSentrySidecarHostedServiceTests
    {
        public class OnStarting
        {
            [Test]
            public async Task Should_wait_for_sidecar()
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
                var host = Substitute.For<IDaprSentryHost>();
                host.GetProcessInfo().Returns(new Process.DaprProcessInfo("TEST", 1234, "VERSION", Process.DaprProcessStatus.Stopped));
                var sidecarHost = Substitute.For<IDaprSidecarHost>();
                var logger = Substitute.For<ILogger<DaprSentrySidecarHostedService>>();
                var hostedService = new DaprSentrySidecarHostedService(sidecarHost, host, optionsAccessor, logger);
                var cancellationToken = CancellationToken.None;

                // Set placement running status after 100ms.
                var timer = new System.Timers.Timer(100);
                timer.Elapsed += (sender, args) => host.GetProcessInfo().Returns(new Process.DaprProcessInfo("TEST", 1234, "VERSION", Process.DaprProcessStatus.Started));
                timer.Start();

                // Start the process
                await hostedService.StartAsync(cancellationToken);

                // Make sure we received a logger call
                var loggerCalls = logger.ReceivedCalls();
                Assert.That(loggerCalls, Is.Not.Empty);
            }

            [Test]
            public async Task Should_not_wait_on_cancellation()
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
                var host = Substitute.For<IDaprSentryHost>();
                host.GetProcessInfo().Returns(new Process.DaprProcessInfo("TEST", 1234, "VERSION", Process.DaprProcessStatus.Stopped));
                var sidecarHost = Substitute.For<IDaprSidecarHost>();
                var logger = Substitute.For<ILogger<DaprSentrySidecarHostedService>>();
                var hostedService = new DaprSentrySidecarHostedService(sidecarHost, host, optionsAccessor, logger);
                var cts = new CancellationTokenSource();

                // Set placement running status after 100ms.
                var timer = new System.Timers.Timer(100);
                timer.Elapsed += (sender, args) => cts.Cancel();
                timer.Start();

                // Start the process
                await hostedService.StartAsync(cts.Token);

                // Make sure we received a logger call
                var loggerCalls = logger.ReceivedCalls();
                Assert.That(loggerCalls, Is.Not.Empty);
            }
        }
    }
}
