using Dapr.Sidekick.DaprClient;
using Dapr.Sidekick.Security;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.Http
{
    public class DaprHttpContextHttpClientFactoryTests
    {
        public class CreateInvocationHandler
        {
            [Test]
            public void Should_create_handler_with_context_accessor()
            {
                var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
                var daprApiTokenAccessor = Substitute.For<IDaprApiTokenAccessor>();

                var factory = new MockDaprHttpContextHttpClientFactory(httpContextAccessor, daprApiTokenAccessor);
                var client = factory.CreateInvokeHttpClient("TEST");
                Assert.That(factory.CreatedInvocationHandler, Is.InstanceOf<HttpContextInvocationHandler>());
            }
        }

        private class MockDaprHttpContextHttpClientFactory : DaprHttpContextHttpClientFactory
        {
            public MockDaprHttpContextHttpClientFactory(IHttpContextAccessor httpContextAccessor, IDaprApiTokenAccessor daprApiTokenAccessor)
                : base(httpContextAccessor, daprApiTokenAccessor)
            {
            }

            public InvocationHandler CreatedInvocationHandler { get; private set; }

            protected override InvocationHandler CreateInvocationHandler()
            {
                var handler = base.CreateInvocationHandler();
                CreatedInvocationHandler = handler;
                return handler;
            }
        }
    }
}
