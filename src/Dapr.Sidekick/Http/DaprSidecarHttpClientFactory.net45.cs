#if !NET35
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using Dapr.Sidekick.DaprClient;
using Dapr.Sidekick.Security;

namespace Dapr.Sidekick.Http
{
    public partial class DaprSidecarHttpClientFactory
    {
        private readonly string _directClientKey = Guid.NewGuid().ToString();
        private readonly ConcurrentDictionary<string, HttpClient> _activeClients = new ConcurrentDictionary<string, HttpClient>(StringComparer.OrdinalIgnoreCase);
        private readonly IDaprApiTokenAccessor _tokenAccessor;
        private string _currentApiToken = null;

        public DaprSidecarHttpClientFactory(IDaprApiTokenAccessor tokenAccessor)
        {
            _tokenAccessor = tokenAccessor ?? throw new ArgumentNullException(nameof(tokenAccessor));
        }

        public HttpClient CreateDaprHttpClient() =>
            CreateClient(
                _directClientKey,
                _ => new HttpClient(new DirectInvocationHandler(_tokenAccessor)
                {
                    InnerHandler = CreateInnerHandler() ?? new HttpClientHandler()
                }));

        public HttpClient CreateInvokeHttpClient(string appId) =>
            CreateClient(
                appId,
                id => DaprClient.DaprClient.CreateInvokeHttpClient(
                    appId: id,
                    daprApiToken: _currentApiToken,
                    createInvocationHandler: () => CreateInvocationHandler(),
                    createInnerHandler: () => CreateInnerHandler()));

        protected virtual InvocationHandler CreateInvocationHandler() => null;

        protected virtual HttpMessageHandler CreateInnerHandler() => null;

        protected override void OnDisposing(bool disposing)
        {
            ClearClients();

            base.OnDisposing(disposing);
        }

        private HttpClient CreateClient(string appId, Func<string, HttpClient> create)
        {
            EnsureNotDisposed();

            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }

            // See if the Api Token has changed since the last request
            var apiToken = _tokenAccessor.DaprApiToken;
            if (_currentApiToken != apiToken)
            {
                // Clear out all clients
                ClearClients();

                // Assign the token
                _currentApiToken = apiToken;
            }

            // Get existing or add a new one
            return _activeClients.GetOrAdd(appId, id => create(id));
        }

        private void ClearClients()
        {
            try
            {
                // Try to remove each gracefully
                foreach (var key in _activeClients.Keys)
                {
                    if (_activeClients.TryRemove(key, out var client))
                    {
                        client.Dispose();
                    }
                }
            }
            finally
            {
                _activeClients.Clear();
            }
        }
    }
}
#endif
