#if !NET35
using System;
using System.Net.Http;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.DaprClient
{
    public class InvocationHandlerTests
    {
        public class Constructor
        {
            [Test]
            public void Should_initialize_properties()
            {
                var handler = new InvocationHandler();
                Assert.That(handler.DaprEndpoint, Is.EqualTo("http://127.0.0.1:3500"));
                Assert.That(handler.DaprApiToken, Is.Null);
            }
        }

        public class DaprEndpoint
        {
            [Test]
            public void Should_throw_exception_when_setting_null()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("value"),
                    () =>
                    {
                        var handler = new InvocationHandler();
                        handler.DaprEndpoint = null;
                    });
            }

            [Test]
            public void Should_throw_exception_when_invalid_scheme()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentException>().With.Message.StartsWith("The URI scheme of the Dapr endpoint must be http or https"),
                    () =>
                    {
                        var handler = new InvocationHandler();
                        handler.DaprEndpoint = "grpc://something";
                    });
            }
        }

        public class SendAsync
        {
            [Test]
            public void Should_throw_exception_when_invalid_uri()
            {
                Assert.Throws(
                    Is.InstanceOf<AggregateException>(),
                    () =>
                    {
                        var handler = new InvocationHandler();
                        handler.Send(new HttpRequestMessage(HttpMethod.Get, "!£$%^&"));
                    });

            }
        }
    }
}
#endif
