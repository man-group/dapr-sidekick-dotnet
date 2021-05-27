using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore.Metrics
{
    public class PrometheusCollectorRegistryTests
    {
        public class CollectAndExportAsTextAsync
        {
            [Test]
            public async Task Should_do_nothing_when_null_collectors()
            {
                var registry = new PrometheusCollectorRegistry(null, null);
                await registry.CollectAndExportAsTextAsync(null);
            }

            [Test]
            public void Should_throw_exception_when_null_serializer()
            {
                var collector1 = Substitute.For<IPrometheusCollector>();
                var registry = new PrometheusCollectorRegistry(new[] { collector1 }, null);

                Assert.ThrowsAsync(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("serializer"),
                    () => registry.CollectAndExportAsTextAsync(null));
            }

            [Test]
            public async Task Should_collect_and_serialize()
            {
                var collector1 = Substitute.For<IPrometheusCollector>();
                var registry = new PrometheusCollectorRegistry(new[] { collector1 }, null);
                var serializer = Substitute.For<IDaprMetricsSerializer>();
                var cts = new CancellationTokenSource();

                collector1.When(x => x.CollectTextAsync(Arg.Any<PrometheusModel>(), cts.Token)).Do(ci =>
                {
                    var model = (PrometheusModel)ci[0];
                    var metric = new PrometheusModel.Metric("METRIC KEY");
                    metric.HelpLine = "HELP LINE";
                    metric.TypeLine = "TYPE LINE";
                    metric.MetricLines.Add("METRIC LINE 1");
                    metric.MetricLines.Add("METRIC LINE 2");
                    model.Metrics.Add(metric.Name, metric);
                });

                var values = new List<string>();
                serializer.When(x => x.WriteLineAsync(Arg.Any<string>(), cts.Token)).Do(ci =>
                {
                    var value = (string)ci[0];
                    values.Add(value);
                });

                await registry.CollectAndExportAsTextAsync(serializer, cts.Token);

                await serializer.Received(1).FlushAsync(cts.Token);

                Assert.That(values.Count, Is.EqualTo(4));
                Assert.That(values[0], Is.EqualTo("HELP LINE"));
                Assert.That(values[1], Is.EqualTo("TYPE LINE"));
                Assert.That(values[2], Is.EqualTo("METRIC LINE 1"));
                Assert.That(values[3], Is.EqualTo("METRIC LINE 2"));
            }

            [Test]
            public async Task Should_exclude_by_filter()
            {
                var collector1 = Substitute.For<IPrometheusCollector>();
                var filter1 = Substitute.For<IPrometheusMetricFilter>();
                var registry = new PrometheusCollectorRegistry(new[] { collector1 }, new[] { filter1 });
                var serializer = Substitute.For<IDaprMetricsSerializer>();
                var cts = new CancellationTokenSource();

                filter1.ExcludeMetricLine("METRIC KEY", "FILTER ME").Returns(true);
                collector1.When(x => x.CollectTextAsync(Arg.Any<PrometheusModel>(), cts.Token)).Do(ci =>
                {
                    var model = (PrometheusModel)ci[0];
                    var metric = new PrometheusModel.Metric("METRIC KEY");
                    metric.HelpLine = "HELP LINE";
                    metric.TypeLine = "TYPE LINE";
                    metric.MetricLines.Add("FILTER ME");
                    metric.MetricLines.Add("METRIC LINE 2");
                    model.Metrics.Add(metric.Name, metric);
                });

                var values = new List<string>();
                serializer.When(x => x.WriteLineAsync(Arg.Any<string>(), cts.Token)).Do(ci =>
                {
                    var value = (string)ci[0];
                    values.Add(value);
                });

                await registry.CollectAndExportAsTextAsync(serializer, cts.Token);

                await serializer.Received(1).FlushAsync(cts.Token);

                Assert.That(values.Count, Is.EqualTo(3));
                Assert.That(values[0], Is.EqualTo("HELP LINE"));
                Assert.That(values[1], Is.EqualTo("TYPE LINE"));
                Assert.That(values[2], Is.EqualTo("METRIC LINE 2"));
            }
        }
    }
}
