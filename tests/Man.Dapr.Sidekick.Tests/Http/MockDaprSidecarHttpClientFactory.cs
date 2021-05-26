#if !NET35
using System.Net.Http;
using Man.Dapr.Sidekick.Security;

namespace Man.Dapr.Sidekick.Http
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
