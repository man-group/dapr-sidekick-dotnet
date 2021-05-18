using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Sidekick.Process;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore
{
    public class DaprProcessHealthCheckTests
    {
        public class CheckHealthAsync
        {
            [Test]
            public async Task Should_return_unhealthy_when_not_running()
            {
                var cancellationToken = CancellationToken.None;
                var host = Substitute.For<IDaprProcessHost>();
                var processInfo = new DaprProcessInfo("P1", 100, "1234", DaprProcessStatus.Stopped);
                host.GetProcessInfo().Returns(processInfo);
                var hc = new MockDaprProcessHealthCheck(host);
                var context = new HealthCheckContext
                {
                    Registration = new HealthCheckRegistration("REG1", hc, HealthStatus.Unhealthy, null)
                };

                var result = await hc.CheckHealthAsync(context, cancellationToken);
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
                Assert.That(result.Description, Is.EqualTo("Dapr process 'P1' not available, status is Stopped"));
            }

            [Test]
            public async Task Should_return_unhealthy_when_unhealthy()
            {
                var cancellationToken = CancellationToken.None;
                var host = Substitute.For<IDaprProcessHost>();
                var processInfo = new DaprProcessInfo("P1", 100, "1234", DaprProcessStatus.Started);
                host.GetProcessInfo().Returns(processInfo);
                host.GetHealthAsync(cancellationToken).Returns(Task.FromResult(new DaprHealthResult(System.Net.HttpStatusCode.NotFound)));
                var hc = new MockDaprProcessHealthCheck(host);
                var context = new HealthCheckContext
                {
                    Registration = new HealthCheckRegistration("REG1", hc, HealthStatus.Unhealthy, null)
                };

                var result = await hc.CheckHealthAsync(context, cancellationToken);
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
                Assert.That(result.Description, Is.EqualTo("Dapr process health check endpoint reports Unhealthy (NotFound)"));
            }

            [Test]
            public async Task Should_return_unhealthy_when_exception()
            {
                var ex = new Exception("TEST_EXCEPTION");
                var cancellationToken = CancellationToken.None;
                var host = Substitute.For<IDaprProcessHost>();
                var processInfo = new DaprProcessInfo("P1", 100, "1234", DaprProcessStatus.Started);
                host.When(x => x.GetProcessInfo()).Do(_ => throw ex);
                host.GetHealthAsync(cancellationToken).Returns(Task.FromResult(new DaprHealthResult(System.Net.HttpStatusCode.NotFound)));
                var hc = new MockDaprProcessHealthCheck(host);
                var context = new HealthCheckContext
                {
                    Registration = new HealthCheckRegistration("REG1", hc, HealthStatus.Unhealthy, null)
                };

                var result = await hc.CheckHealthAsync(context, cancellationToken);
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
                Assert.That(result.Exception, Is.SameAs(ex));
            }

            [Test]
            public async Task Should_return_healthy()
            {
                var cancellationToken = CancellationToken.None;
                var host = Substitute.For<IDaprProcessHost>();
                var processInfo = new DaprProcessInfo("P1", 100, "1234", DaprProcessStatus.Started);
                host.GetProcessInfo().Returns(processInfo);
                host.GetHealthAsync(cancellationToken).Returns(Task.FromResult(new DaprHealthResult(System.Net.HttpStatusCode.OK)));
                var hc = new MockDaprProcessHealthCheck(host);
                var context = new HealthCheckContext
                {
                    Registration = new HealthCheckRegistration("REG1", hc, HealthStatus.Unhealthy, null)
                };

                var result = await hc.CheckHealthAsync(context, cancellationToken);
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
                Assert.That(result.Description, Is.EqualTo("Dapr process 'P1' running, version 1234"));
            }
        }

        private class MockDaprProcessHealthCheck : DaprProcessHealthCheck
        {
            public MockDaprProcessHealthCheck(IDaprProcessHost daprProcessHost)
                : base(daprProcessHost)
            {
            }
        }
    }
}
