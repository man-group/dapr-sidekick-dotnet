#if NET35
using System;
using System.Net;
using Man.Dapr.Sidekick.Security;

namespace Man.Dapr.Sidekick.Http
{
    public partial class DaprSidecarHttpClientFactory
    {
        private readonly IDaprApiTokenAccessor _tokenAccessor;

        public DaprSidecarHttpClientFactory(IDaprApiTokenAccessor tokenAccessor)
        {
            _tokenAccessor = tokenAccessor ?? throw new ArgumentNullException(nameof(tokenAccessor));
        }

        public WebClient CreateDaprWebClient()
        {
            EnsureNotDisposed();

            // Create a client
            var client = new WebClient
            {
                // The base address internally always has a final "/"
                // added to the end if it does not already exist.
                BaseAddress = GetDefaultDaprEndpoint()
            };

            // Add the API token header
            var apiToken = _tokenAccessor.DaprApiToken;
            if (!string.IsNullOrEmpty(apiToken))
            {
                client.Headers[DaprConstants.DaprApiTokenHeader] = apiToken;
            }

            return client;
        }

        public WebClient CreateInvokeWebClient(string appId)
        {
            EnsureNotDisposed();

            if (appId.IsNullOrWhiteSpaceEx())
            {
                throw new ArgumentNullException(nameof(appId));
            }

            var client = CreateDaprWebClient();
            client.BaseAddress += $"v1.0/invoke/{appId}/method";
            return client;
        }
    }
}
#endif
