#if NET35
using System;
using NUnit.Framework;

namespace Dapr.Sidekick.Http
{
    public class DaprProcessHttpClientFactoryTests
    {
        public class CreateDaprWebClient
        {
            [Test]
            public void Should_create_unique_instance_each_call()
            {
                var factory = new DaprProcessHttpClientFactory();
                Assert.That(factory.CreateDaprWebClient(), Is.Not.SameAs(factory.CreateDaprWebClient()));
            }
        }

        [Test]
        public void Should_throw_exception_when_disposed()
        {
            var factory = new DaprProcessHttpClientFactory();
            factory.Dispose();
            Assert.Throws(Is.TypeOf<ObjectDisposedException>(), () => factory.CreateDaprWebClient());
        }
    }
}
#endif
