using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public class DaprMetricsTextSerializerTests
    {
        public class FlushAsync
        {
            [Test]
            public void Should_not_flush_when_stream_not_created()
            {
                var ms = new MemoryStream();
                var serializer = new DaprMetricsTextSerializer(() => ms);
                var cts = new CancellationTokenSource();

                Assert.That(serializer.FlushAsync(cts.Token), Is.EqualTo(Task.CompletedTask));
            }

            [Test]
            public async Task Should_flush_when_stream_has_content()
            {
                var ms = new MemoryStream();
                var serializer = new DaprMetricsTextSerializer(() => ms);
                var cts = new CancellationTokenSource();

                await serializer.WriteLineAsync("TEST", cts.Token);
                await serializer.FlushAsync(cts.Token);
            }
        }

        public class WriteLineAsync
        {
            [TestCase(null, "")]
            [TestCase("", "")]
            [TestCase("TEST", "TEST\n")]
            public async Task Should_write_expected_value(string value, string expected)
            {
                var ms = new MemoryStream();
                var serializer = new DaprMetricsTextSerializer(ms);
                var cts = new CancellationTokenSource();

                await serializer.WriteLineAsync(value, cts.Token);
                Assert.That(System.Text.Encoding.UTF8.GetString(ms.ToArray()), Is.EqualTo(expected));
            }
        }
    }
}
