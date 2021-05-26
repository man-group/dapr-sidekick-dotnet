#if !NET35
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Man.Dapr.Sidekick.Security;

namespace Man.Dapr.Sidekick.DaprClient
{
    internal class DirectInvocationHandler : DelegatingHandler
    {
        private readonly IDaprApiTokenAccessor _tokenAccessor;

        public DirectInvocationHandler(IDaprApiTokenAccessor tokenAccessor)
        {
            _tokenAccessor = tokenAccessor;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiTokenHeader = DaprClient.GetDaprApiTokenHeader(_tokenAccessor.DaprApiToken);
            var isValidToken = apiTokenHeader.Key != null && apiTokenHeader.Value != null;

            try
            {
                if (isValidToken)
                {
                    request.Headers.Add(apiTokenHeader.Key, apiTokenHeader.Value);
                }
                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                if (isValidToken)
                {
                    request.Headers.Remove(apiTokenHeader.Key);
                }
            }
        }
    }
}
#endif
