using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarHealthCheckTests
    {
        public class CheckHealthAsync
        {
            [Test]
            public async Task Should_get_health_from_host()
            {
                var host = Substitute.For<IDaprSidecarHost>();
                var hc = new DaprSidecarHealthCheck(host);

                var result = await hc.CheckHealthAsync(null);

                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
                host.Received(1).GetProcessInfo();
            }
        }
    }
}
