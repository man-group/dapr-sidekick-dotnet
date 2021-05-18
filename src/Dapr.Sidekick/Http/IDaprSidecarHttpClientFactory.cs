namespace Dapr.Sidekick.Http
{
    public interface IDaprSidecarHttpClientFactory
    {
#if NET35
        /// <summary>
        /// Creates a <see cref="System.Net.WebClient"/> instance for invoking remote methods on the specified <paramref name="appId"/>.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="System.Net.WebClient"/> will have its <see cref="System.Net.WebClient.BaseAddress"/> set to the Dapr Sidecar service invocation initial path.
        /// For example given a <paramref name="appId"/> value of "test-app" the Base Address would be set to a value
        /// similar to http://127.0.0.1:3500/v1.0/invoke/test-app/method/ (note the trailing slash).
        /// If a Dapr API Token has been configured it will be added to all calls.
        /// </remarks>
        /// <param name="appId">The Dapr appId for the target service.</param>
        /// <returns>A <see cref="System.Net.WebClient"/> instance with a <see cref="System.Net.WebClient.BaseAddress"/> set to the Dapr Sidecar service invocation initial path.</returns>
        System.Net.WebClient CreateInvokeWebClient(string appId);
#else
        /// <summary>
        /// Creates a <see cref="System.Net.Http.HttpClient"/> instance for invoking remote methods on the specified <paramref name="appId"/>.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="System.Net.Http.HttpClient"/> will be created with a special <see cref="System.Net.Http.DelegatingHandler"/> which
        /// will transform any request Uri to the necessary Dapr Sidecar service invocation address.
        /// For example given a <paramref name="appId"/> value of "test-app" the Base Address would be set to a value
        /// similar to http://127.0.0.1:3500/v1.0/invoke/test-app/method/ (note the trailing slash).
        /// If a Dapr API Token has been configured it will be added to all calls.
        /// </remarks>
        /// <param name="appId">The Dapr appId for the target service.</param>
        /// <returns>A <see cref="System.Net.Http.HttpClient"/> instance suitable for Dapr service invocation.</returns>
        System.Net.Http.HttpClient CreateInvokeHttpClient(string appId);
#endif
    }
}
