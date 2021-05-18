using Dapr.Sidekick.DaprClient;
using Dapr.Sidekick.Security;
using Microsoft.AspNetCore.Http;

namespace Dapr.Sidekick.Http
{
    internal class DaprHttpContextHttpClientFactory : DaprSidecarHttpClientFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DaprHttpContextHttpClientFactory(
            IHttpContextAccessor httpContextAccessor,
            IDaprApiTokenAccessor daprApiTokenAccessor)
            : base(daprApiTokenAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override InvocationHandler CreateInvocationHandler() => new HttpContextInvocationHandler(_httpContextAccessor);
    }
}
