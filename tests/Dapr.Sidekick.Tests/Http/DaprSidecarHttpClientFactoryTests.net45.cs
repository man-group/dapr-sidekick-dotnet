#if !NET35
using System;
using System.Threading.Tasks;
using Dapr.Sidekick.Security;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.Http
{
    public class DaprSidecarHttpClientFactoryTests
    {
        public class CreateDaprHttpClient
        {
            [Test]
            public void Should_reuse_same_instance_each_call()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new MockDaprSidecarHttpClientFactory(tokenAccessor);
                Assert.That(factory.CreateDaprHttpClient(), Is.SameAs(factory.CreateDaprHttpClient()));
            }

            [Test]
            public async Task Should_not_add_api_token_header()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new MockDaprSidecarHttpClientFactory(tokenAccessor);
                factory.InnerHandler.OnSendAsyncRequest = request =>
                {
                    Assert.That(request.Headers.TryGetValues(DaprConstants.DaprApiTokenHeader, out var values), Is.False);
                };

                var client = factory.CreateDaprHttpClient();
                await client.GetAsync("http://does-not-exist");

                Assert.That(factory.InnerHandler.SendAsyncRequest, Is.Not.Null);
            }

            [Test]
            public async Task Should_add_api_token_header()
            {
                var token = new SensitiveString("TEST_VALUE");
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                tokenAccessor.DaprApiToken.Returns(token);
                var factory = new MockDaprSidecarHttpClientFactory(tokenAccessor);
                factory.InnerHandler.OnSendAsyncRequest = request =>
                {
                    Assert.That(request.Headers.TryGetValues(DaprConstants.DaprApiTokenHeader, out var values), Is.True);
                    Assert.That(values, Does.Contain(token.Value));
                };

                var client = factory.CreateDaprHttpClient();
                await client.GetAsync("http://does-not-exist");

                Assert.That(factory.InnerHandler.SendAsyncRequest, Is.Not.Null);
            }

            [Test]
            public async Task Should_invoke_with_expected_uri()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new MockDaprSidecarHttpClientFactory(tokenAccessor);
                factory.InnerHandler.OnSendAsyncRequest = request =>
                {
                    Assert.That(request.RequestUri.ToString(), Is.EqualTo("http://does-not-exist/"));
                };

                var client = factory.CreateDaprHttpClient();
                await client.GetAsync("http://does-not-exist");

                Assert.That(factory.InnerHandler.SendAsyncRequest, Is.Not.Null);
            }

            [Test]
            public void Should_create_new_instance_when_api_token_changes()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                tokenAccessor.DaprApiToken.Returns(new SensitiveString("TEST_VALUE"));
                var factory = new MockDaprSidecarHttpClientFactory(tokenAccessor);

                var client1 = factory.CreateDaprHttpClient();
                tokenAccessor.DaprApiToken.Returns(new SensitiveString("TEST_VALUE_2"));

                Assert.That(client1, Is.Not.SameAs(factory.CreateDaprHttpClient()));
            }

            [Test]
            public void Should_throw_exception_when_disposed()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new DaprSidecarHttpClientFactory(tokenAccessor);
                factory.Dispose();
                Assert.Throws(Is.TypeOf<ObjectDisposedException>(), () => factory.CreateDaprHttpClient());
            }
        }

        public class CreateInvokeHttpClientMethod
        {
            [Test]
            public void Should_throw_exception_when_invalid_appid()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new MockDaprSidecarHttpClientFactory(tokenAccessor);

                Assert.Throws(Is.InstanceOf<ArgumentNullException>(), () => factory.CreateInvokeHttpClient(null));
                Assert.Throws(Is.InstanceOf<ArgumentNullException>(), () => factory.CreateInvokeHttpClient(string.Empty));
                Assert.Throws(Is.InstanceOf<ArgumentNullException>(), () => factory.CreateInvokeHttpClient("  "));
            }

            [Test]
            public async Task Should_not_add_api_token_header()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new MockDaprSidecarHttpClientFactory(tokenAccessor);
                factory.InnerHandler.OnSendAsyncRequest = request =>
                {
                    Assert.That(request.Headers.TryGetValues(DaprConstants.DaprApiTokenHeader, out var values), Is.False);
                };

                var client = factory.CreateInvokeHttpClient("test-app");
                await client.GetAsync("http://does-not-exist");

                Assert.That(factory.InnerHandler.SendAsyncRequest, Is.Not.Null);
            }

            [Test]
            public async Task Should_add_api_token_header()
            {
                var token = new SensitiveString("TEST_VALUE");
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                tokenAccessor.DaprApiToken.Returns(token);
                var factory = new MockDaprSidecarHttpClientFactory(tokenAccessor);
                factory.InnerHandler.OnSendAsyncRequest = request =>
                {
                    Assert.That(request.Headers.TryGetValues(DaprConstants.DaprApiTokenHeader, out var values), Is.True);
                    Assert.That(values, Does.Contain(token.Value));
                };

                var client = factory.CreateInvokeHttpClient("test-app");
                await client.GetAsync("http://does-not-exist");

                Assert.That(factory.InnerHandler.SendAsyncRequest, Is.Not.Null);
            }

            [Test]
            public async Task Should_invoke_with_rewritten_uri()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new MockDaprSidecarHttpClientFactory(tokenAccessor);
                factory.InnerHandler.OnSendAsyncRequest = request =>
                {
                    Assert.That(request.RequestUri.ToString(), Is.EqualTo("http://127.0.0.1:3500/v1.0/invoke/test-app/method/some/api"));
                };

                var client = factory.CreateInvokeHttpClient("test-app");
                await client.GetAsync("/some/api");

                Assert.That(factory.InnerHandler.SendAsyncRequest, Is.Not.Null);
            }

            [Test]
            public void Should_throw_exception_when_disposed()
            {
                var tokenAccessor = Substitute.For<IDaprApiTokenAccessor>();
                var factory = new DaprSidecarHttpClientFactory(tokenAccessor);
                factory.Dispose();
                Assert.Throws(Is.TypeOf<ObjectDisposedException>(), () => factory.CreateInvokeHttpClient("test-app"));
            }
        }
    }
}
#endif
