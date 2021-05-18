using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Sidekick.Process;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public class DaprProcessHostPrometheusCollectorTests
    {
        public class CollectTextAsync
        {
            [Test]
            public void Should_throw_exception_when_null_model()
            {
                Assert.ThrowsAsync(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("model"),
                    () => new MockDaprProcessHostPrometheusCollector(null).CollectTextAsync(null));
            }

            [Test]
            public async Task Should_do_nothing_if_host_not_running()
            {
                var host = Substitute.For<IDaprProcessHost>();
                var collector = new MockDaprProcessHostPrometheusCollector(host);
                var model = new PrometheusModel();

                await collector.CollectTextAsync(model);

                host.Received(1).GetProcessInfo();
                host.DidNotReceive().GetProcessOptions();
            }

            [Test]
            public async Task Should_do_nothing_if_collector_disabled()
            {
                var host = Substitute.For<IDaprProcessHost>();
                var collector = new MockDaprProcessHostPrometheusCollector(host);
                var model = new PrometheusModel();
                var cts = new CancellationTokenSource();
                host.GetProcessInfo().Returns(new DaprProcessInfo("TEST", 1234, "VERSION", DaprProcessStatus.Started));
                host.GetProcessOptions().Returns(new MockDaprProcessOptions
                {
                    Metrics = new DaprMetricsOptions
                    {
                        EnableCollector = false
                    }
                });
                await collector.CollectTextAsync(model, cts.Token);

                host.Received(1).GetProcessInfo();
                await host.DidNotReceive().WriteMetricsAsync(Arg.Any<Stream>(), cts.Token);
            }

            [Test]
            public async Task Should_not_update_model_when_no_metrics()
            {
                var host = Substitute.For<IDaprProcessHost>();
                var collector = new MockDaprProcessHostPrometheusCollector(host);
                var model = new PrometheusModel();
                var cts = new CancellationTokenSource();
                var options = new DaprOptions
                {
                    Metrics = new DaprMetricsOptions()
                };
                host.GetProcessInfo().Returns(new DaprProcessInfo("TEST", 1234, "VERSION", DaprProcessStatus.Started));
                host.GetProcessOptions().Returns(options);
                await collector.CollectTextAsync(model, cts.Token);

                host.Received(1).GetProcessInfo();
                host.WriteMetricsAsync(Arg.Any<Stream>(), cts.Token).Returns(0);
                Assert.That(model.Metrics, Is.Empty);
            }

            [Test]
            public async Task Should_update_model()
            {
                var sb = new StringBuilder();
                sb.AppendLine("# HELP dapr_http_client_completed_count Count of completed requests");
                sb.AppendLine("# TYPE dapr_http_client_completed_count counter");
                sb.AppendLine("dapr_http_client_completed_count{app_id=\"dapr-placement\",method=\"GET\",path=\"dapr/config\",status=\"404\" } 1");
                sb.AppendLine("# HELP dapr_http_server_request_bytes_bucket Size distribution of HTTP request body");
                sb.AppendLine("# TYPE dapr_http_server_request_bytes_bucket histogram");
                sb.AppendLine("dapr_http_server_request_bytes_bucket{app_id=\"dapr-placement\",le=\"1024\"} 92118");
                sb.AppendLine("dapr_http_server_request_bytes_bucket{app_id=\"dapr-placement\",le=\"2048\"} 92122");
                var content = Encoding.UTF8.GetBytes(sb.ToString());

                var host = Substitute.For<IDaprProcessHost>();
                var collector = new MockDaprProcessHostPrometheusCollector(host);
                var model = new PrometheusModel();
                var cts = new CancellationTokenSource();
                var options = new DaprOptions
                {
                    Metrics = new DaprMetricsOptions()
                };
                host.GetProcessInfo().Returns(new DaprProcessInfo("TEST", 1234, "VERSION", DaprProcessStatus.Started));
                host.GetProcessOptions().Returns(options);
                host.When(x => x.WriteMetricsAsync(Arg.Any<Stream>(), cts.Token)).Do(ci =>
                {
                    var stream = (Stream)ci[0];
                    stream.Write(content);
                });
                host.WriteMetricsAsync(Arg.Any<Stream>(), cts.Token).Returns(content.Length);

                await collector.CollectTextAsync(model, cts.Token);

                host.Received(1).GetProcessInfo();
                Assert.That(model.Metrics.Count, Is.EqualTo(2));
            }
        }

        private class MockDaprProcessHostPrometheusCollector : DaprProcessHostPrometheusCollector
        {
            public MockDaprProcessHostPrometheusCollector(IDaprProcessHost daprProcessHost)
                : base(daprProcessHost)
            {
            }
        }
    }
}
