namespace Man.Dapr.Sidekick.Security
{
    public interface IDaprApiTokenAccessor
    {
        /// <summary>
        /// Gets the API Token that Dapr will expect to see in the header of each public API request to authenticate the caller.
        /// The Dapr process will be launched with the environment variable DAPR_API_TOKEN set to this value.
        /// See https://docs.dapr.io/operations/security/api-token/.
        /// </summary>
        public SensitiveString DaprApiToken { get; }

        /// <summary>
        /// Gets the API Token that Dapr will provide as a header in each request sent to an application from the sidecar.
        /// The Dapr process will be launched with the environment variable APP_API_TOKEN set to this value.
        /// See https://docs.dapr.io/operations/security/app-api-token/.
        /// </summary>
        public SensitiveString AppApiToken { get; }
    }
}
