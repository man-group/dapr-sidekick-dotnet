using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Man.Dapr.Sidekick.AspNetCore.Metrics
{
    public class PrometheusTextReaderTests
    {
        public class Constructor
        {
            [Test]
            public void Should_throw_exception_when_null_model()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("model"),
                    () => new PrometheusTextReader(null));
            }
        }

        public class ReadAsync
        {
            [Test]
            public async Task Should_read_all_line_types()
            {
                var sb = new StringBuilder();
                sb.AppendLine("# HELP METRIC_1 DESCRIPTION");
                sb.AppendLine("# TYPE METRIC_1 DESCRIPTION");
                sb.AppendLine(string.Empty);
                sb.AppendLine("NONAME");
                sb.AppendLine("# UNKNOWN");
                sb.AppendLine("METRIC_1 METRIC LINE");
                sb.AppendLine("METRIC_2 METRIC LINE");
                var content = Encoding.UTF8.GetBytes(sb.ToString());
                var cts = new CancellationTokenSource();

                var ms = new MemoryStream();
                ms.Write(content);
                ms.Position = 0;

                var model = new PrometheusModel();
                var reader = new PrometheusTextReader(model);
                await reader.ReadAsync(ms, cts.Token);

                Assert.That(model.Unknown.Count, Is.EqualTo(3));
                Assert.That(model.Unknown[0], Is.EqualTo(string.Empty));
                Assert.That(model.Unknown[1], Is.EqualTo("NONAME"));
                Assert.That(model.Unknown[2], Is.EqualTo("# UNKNOWN"));
                Assert.That(model.Metrics.Count, Is.EqualTo(2));

                var metric1 = model.Metrics["METRIC_1"];
                Assert.That(metric1.HelpLine, Is.EqualTo("# HELP METRIC_1 DESCRIPTION"));
                Assert.That(metric1.TypeLine, Is.EqualTo("# TYPE METRIC_1 DESCRIPTION"));
                Assert.That(metric1.MetricLines.Count, Is.EqualTo(1));
                Assert.That(metric1.MetricLines[0], Is.EqualTo("METRIC_1 METRIC LINE"));

                var metric2 = model.Metrics["METRIC_2"];
                Assert.That(metric2.HelpLine, Is.Null);
                Assert.That(metric2.TypeLine, Is.Null);
                Assert.That(metric2.MetricLines.Count, Is.EqualTo(1));
                Assert.That(metric2.MetricLines[0], Is.EqualTo("METRIC_2 METRIC LINE"));
            }
        }
    }
}
