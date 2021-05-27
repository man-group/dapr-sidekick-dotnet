using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Man.Dapr.Sidekick.Http;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore.Metrics
{
    public class DaprMetricsServerMiddlewareTests
    {
        public class Invoke
        {
            [Test]
            public async Task Should_collect_and_export()
            {
                var registry = Substitute.For<IDaprMetricsCollectorRegistry>();
                var middleware = new DaprMetricsServerMiddleware(null, registry);
                var ms = new MemoryStream();
                var response = new MockHttpResponse
                {
                    Body = ms
                };
                var context = new MockHttpContext(response: response);

                registry.When(x => x.CollectAndExportAsTextAsync(Arg.Any<IDaprMetricsSerializer>(), context.RequestAborted)).Do(ci =>
                {
                    var serializer = (DaprMetricsTextSerializer)ci[0];
                    serializer.WriteLineAsync("TEST", context.RequestAborted).Wait();
                });

                await middleware.Invoke(context);

                Assert.That(response.ContentType, Is.EqualTo(DaprMetricsConstants.ExporterContentType));
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                Assert.That(System.Text.Encoding.UTF8.GetString(ms.ToArray()), Is.EqualTo("TEST\n"));
            }

            [Test]
            public async Task Should_catch_exception_when_cancelled()
            {
                var registry = Substitute.For<IDaprMetricsCollectorRegistry>();
                var middleware = new DaprMetricsServerMiddleware(null, registry);
                var ms = new MemoryStream();
                var cts = new CancellationTokenSource();
                var response = new MockHttpResponse
                {
                    Body = ms
                };
                var context = new MockHttpContext(response: response)
                {
                    RequestAborted = cts.Token
                };

                registry.When(x => x.CollectAndExportAsTextAsync(Arg.Any<IDaprMetricsSerializer>(), context.RequestAborted)).Do(_ =>
                {
                    // Trigger cancellation
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                });

                await middleware.Invoke(context);

                // Should not have serialized
                Assert.That(response.ContentType, Is.Null);
            }

            [Test]
            public async Task Should_write_exception_when_thrown()
            {
                var registry = Substitute.For<IDaprMetricsCollectorRegistry>();
                var middleware = new DaprMetricsServerMiddleware(null, registry);
                var ms = new MemoryStream();
                var ex = new Exception("EXCEPTION_MESSAGE");
                var response = new MockHttpResponse
                {
                    Body = ms
                };
                var context = new MockHttpContext(response: response);
                registry.When(x => x.CollectAndExportAsTextAsync(Arg.Any<IDaprMetricsSerializer>(), context.RequestAborted)).Do(_ =>
                {
                    // Throw
                    throw ex;
                });

                await middleware.Invoke(context);

                // Should serialize exception
                Assert.That(response.ContentType, Is.Null);
                Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
                Assert.That(System.Text.Encoding.UTF8.GetString(ms.ToArray()), Is.EqualTo("EXCEPTION_MESSAGE"));
            }
        }
    }
}
