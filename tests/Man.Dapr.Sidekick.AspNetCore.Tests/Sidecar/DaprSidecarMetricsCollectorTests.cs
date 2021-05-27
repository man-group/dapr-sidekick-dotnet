using System.Threading.Tasks;
using Man.Dapr.Sidekick.AspNetCore.Metrics;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarMetricsCollectorTests
    {
        public class CollectTextAsync
        {
            [Test]
            public async Task Should_get_info_from_host()
            {
                var host = Substitute.For<IDaprSidecarHost>();
                var collector = new DaprSidecarMetricsCollector(host);
                var model = new PrometheusModel();

                await collector.CollectTextAsync(model);

                host.Received(1).GetProcessInfo();
            }
        }
    }
}
