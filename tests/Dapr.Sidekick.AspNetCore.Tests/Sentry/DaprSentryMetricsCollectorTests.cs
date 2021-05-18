using System.Threading.Tasks;
using Dapr.Sidekick.AspNetCore.Metrics;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Sentry
{
    public class DaprSentryMetricsCollectorTests
    {
        public class CollectTextAsync
        {
            [Test]
            public async Task Should_get_info_from_host()
            {
                var host = Substitute.For<IDaprSentryHost>();
                var collector = new DaprSentryMetricsCollector(host);
                var model = new PrometheusModel();

                await collector.CollectTextAsync(model);

                host.Received(1).GetProcessInfo();
            }
        }
    }
}
