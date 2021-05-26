#if NET35
using System;
using Man.Dapr.Sidekick.Security;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Http
{
    public class DaprSidecarHttpClientFactoryTests
    {
        public class CreateDaprWebClient
        {
            [Test]
            public void Should_create_unique_instance_each_call()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new DaprSidecarHttpClientFactory(tokenAccessor);
                Assert.That(factory.CreateDaprWebClient(), Is.Not.SameAs(factory.CreateDaprWebClient()));
            }

            [Test]
            public void Should_not_add_api_token_header()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new DaprSidecarHttpClientFactory(tokenAccessor);
                var client = factory.CreateDaprWebClient();
                Assert.That(client.Headers[DaprConstants.DaprApiTokenHeader], Is.Null);
            }

            [Test]
            public void Should_add_api_token_header()
            {
                var token = new SensitiveString("TEST_VALUE");
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                tokenAccessor.DaprApiToken.Returns(token);
                var factory = new DaprSidecarHttpClientFactory(tokenAccessor);
                var client = factory.CreateDaprWebClient();
                Assert.That(client.Headers[DaprConstants.DaprApiTokenHeader], Is.EqualTo(token.Value));
            }

            [Test]
            public void Should_throw_exception_when_disposed()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new DaprSidecarHttpClientFactory(tokenAccessor);
                factory.Dispose();
                Assert.Throws(Is.TypeOf<ObjectDisposedException>(), () => factory.CreateDaprWebClient());
            }
        }

        public class CreateInvokeWebClientMethod
        {
            [Test]
            public void Should_throw_exception_when_invalid_appid()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new DaprSidecarHttpClientFactory(tokenAccessor);

                Assert.Throws(Is.InstanceOf<ArgumentNullException>(), () => factory.CreateInvokeWebClient(null));
                Assert.Throws(Is.InstanceOf<ArgumentNullException>(), () => factory.CreateInvokeWebClient(string.Empty));
                Assert.Throws(Is.InstanceOf<ArgumentNullException>(), () => factory.CreateInvokeWebClient("  "));
            }

            [Test]
            public void Should_set_base_address()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new DaprSidecarHttpClientFactory(tokenAccessor);
                var client = factory.CreateInvokeWebClient("test-app");
                Assert.That(client.BaseAddress, Is.EqualTo("http://127.0.0.1:3500/v1.0/invoke/test-app/method"));
            }

            [Test]
            public void Should_throw_exception_when_disposed()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new DaprSidecarHttpClientFactory(tokenAccessor);
                factory.Dispose();
                Assert.Throws(Is.TypeOf<ObjectDisposedException>(), () => factory.CreateInvokeWebClient("test-app"));
            }
        }
    }
}
#endif
