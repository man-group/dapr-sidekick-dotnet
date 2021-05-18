using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Sentry
{
    public class DaprSentryHealthCheckTests
    {
        public class CheckHealthAsync
        {
            [Test]
            public async Task Should_get_health_from_host()
            {
                var host = Substitute.For<IDaprSentryHost>();
                var hc = new DaprSentryHealthCheck(host);

                var result = await hc.CheckHealthAsync(null);

                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
                host.Received(1).GetProcessInfo();
            }
        }
    }
}
