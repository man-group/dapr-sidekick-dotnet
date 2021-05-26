#if !NET35
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Http
{
    public class DaprProcessHttpClientFactoryTests
    {
        public class CreateDaprHttpClient
        {
            [Test]
            public void Should_reuse_same_instance_each_call()
            {
                var factory = new DaprProcessHttpClientFactory();
                Assert.That(factory.CreateDaprHttpClient(), Is.SameAs(factory.CreateDaprHttpClient()));
            }

            [Test]
            public async Task Should_use_specified_handler()
            {
                var handler = new MockHttpClientHandler();
                var factory = new DaprProcessHttpClientFactory(handler);
                var client = factory.CreateDaprHttpClient();
                await client.GetAsync("http://does-not-exist");
                Assert.That(handler.SendAsyncInvoked, Is.True);
            }

            [Test]
            public void Should_throw_exception_when_disposed()
            {
                var factory = new DaprProcessHttpClientFactory();
                factory.Dispose();
                Assert.Throws(Is.TypeOf<ObjectDisposedException>(), () => factory.CreateDaprHttpClient());
            }
        }
    }
}
#endif
