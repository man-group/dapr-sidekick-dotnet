namespace Dapr.Sidekick.Security
{
    public interface IDaprApiTokenManager : IDaprApiTokenAccessor
    {
        /// <summary>
        /// Sets the API Token that Dapr will expect to see in the header of each public API request to authenticate the caller.
        /// The Dapr process will be launched with the environment variable DAPR_API_TOKEN set to this value.
        /// See https://docs.dapr.io/operations/security/api-token/.
        /// </summary>
        /// <param name="apiToken">An API Token.</param>
        public void SetDaprApiToken(SensitiveString apiToken);

        /// <summary>
        /// Sets the API Token that Dapr will provide as a header in each request sent to an application from the sidecar.
        /// The Dapr process will be launched with the environment variable APP_API_TOKEN set to this value.
        /// See https://docs.dapr.io/operations/security/app-api-token/.
        /// </summary>
        /// <param name="apiToken">An API Token.</param>
        public void SetAppApiToken(SensitiveString apiToken);
    }
}
