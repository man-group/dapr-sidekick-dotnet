using System.Threading.Tasks;
using Dapr.Sidekick.AspNetCore.Metrics;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Placement
{
    public class DaprPlacementMetricsCollectorTests
    {
        public class CollectTextAsync
        {
            [Test]
            public async Task Should_get_info_from_host()
            {
                var host = Substitute.For<IDaprPlacementHost>();
                var collector = new DaprPlacementMetricsCollector(host);
                var model = new PrometheusModel();

                await collector.CollectTextAsync(model);

                host.Received(1).GetProcessInfo();
            }
        }
    }
}
