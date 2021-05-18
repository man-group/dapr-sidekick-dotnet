using System;
using System.Net.Http;
using System.Threading;
using Dapr.Sidekick.DaprClient;
using Microsoft.AspNetCore.Http;

namespace Dapr.Sidekick.Http
{
    internal class HttpContextInvocationHandler : InvocationHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextInvocationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override void OnBeforeSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add the authorization token if available
            var context = _httpContextAccessor.HttpContext;
            var authorizationHeaders = context.Request.Headers[HttpHeaderConstants.Authorization];
            if (authorizationHeaders.Count > 0)
            {
                request.Headers.Add(HttpHeaderConstants.Authorization, authorizationHeaders.ToArray());
            }

            base.OnBeforeSendAsync(request, cancellationToken);
        }

        protected override void OnAfterSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Remove the authorization token if previously added
            var context = _httpContextAccessor.HttpContext;
            var authorizationHeaders = context.Request.Headers[HttpHeaderConstants.Authorization];
            if (authorizationHeaders.Count > 0)
            {
                request.Headers.Remove(HttpHeaderConstants.Authorization);
            }

            base.OnAfterSendAsync(request, cancellationToken);
        }
    }
}
