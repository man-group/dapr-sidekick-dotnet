#if !NET35
using System.Net.Http;
using Dapr.Sidekick.Security;

namespace Dapr.Sidekick.Http
{
    public class MockDaprSidecarHttpClientFactory : DaprSidecarHttpClientFactory
    {
        public MockDaprSidecarHttpClientFactory(IDaprApiTokenAccessor tokenAccessor)
            : base(tokenAccessor)
        {
            InnerHandler = new MockHttpClientHandler();
        }

        public MockHttpClientHandler InnerHandler { get; }

        protected override HttpMessageHandler CreateInnerHandler() => base.CreateInnerHandler() ?? InnerHandler;
    }
}
#endif
