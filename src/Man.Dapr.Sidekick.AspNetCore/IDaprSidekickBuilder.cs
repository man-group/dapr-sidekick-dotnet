namespace Man.Dapr.Sidekick.AspNetCore
{
    /// <summary>
    /// A builder used to register additional services for Dapr Sidekick.
    /// </summary>
    public interface IDaprSidekickBuilder
    {
        /// <summary>
        /// Adds the Dapr Placement service.
        /// </summary>
        /// <returns>This instance to allow calls to be chained.</returns>
        public IDaprSidekickBuilder AddPlacement();

        /// <summary>
        /// Adds the Dapr Sentry service.
        /// </summary>
        /// <returns>This instance to allow calls to be chained.</returns>
        public IDaprSidekickBuilder AddSentry();
    }
}
