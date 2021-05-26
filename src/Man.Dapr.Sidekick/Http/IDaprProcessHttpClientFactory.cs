namespace Man.Dapr.Sidekick.Http
{
    /// <summary>
    /// Defines the methods for creating an HttpClient for direct access to the Dapr process endpoints.
    /// For internal use only.
    /// </summary>
    public interface IDaprProcessHttpClientFactory
    {
#if NET35
        /// <summary>
        /// Creates a <see cref="System.Net.WebClient"/> instance for invoking methods on the associated Dapr Process HTTP endpoints.
        /// </summary>
        /// <returns>A <see cref="System.Net.WebClient"/> instance.</returns>
        System.Net.WebClient CreateDaprWebClient();
#else
        /// <summary>
        /// Creates a <see cref="System.Net.Http.HttpClient"/> instance for invoking methods on the associated Dapr Process HTTP endpoints.
        /// </summary>
        /// <returns>A <see cref="System.Net.Http.HttpClient"/> instance.</returns>
        System.Net.Http.HttpClient CreateDaprHttpClient();
#endif
    }
}
