using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Dapr.Sidekick.Http
{
    internal class MockHttpContextInvocationHandler : HttpContextInvocationHandler
    {
        public MockHttpContextInvocationHandler(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }

        public void InvokeBeforeSendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => OnBeforeSendAsync(request, cancellationToken);

        public void InvokeAfterSendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => OnAfterSendAsync(request, cancellationToken);
    }
}
