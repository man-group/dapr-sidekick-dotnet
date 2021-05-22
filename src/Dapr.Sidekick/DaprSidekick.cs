using System;
using System.Collections.Generic;
using Dapr.Sidekick.Http;
using Dapr.Sidekick.Logging;
using Dapr.Sidekick.Process;
using Dapr.Sidekick.Security;

namespace Dapr.Sidekick
{
    /// <summary>
    /// The Dapr Sidekick standalone controller for use outside a Dependency Injection framework.
    /// Created using <see cref="DaprSidekickBuilder"/>.
    /// </summary>
    public class DaprSidekick
    {
        private readonly Lazy<IDaprPlacementHost> _placementHost;
        private readonly Lazy<IDaprSentryHost> _sentryHost;
        private readonly Lazy<IDaprSidecarHost> _sidecarHost;

        internal DaprSidekick()
        {
            _placementHost = new Lazy<IDaprPlacementHost>(() => new DaprPlacementHost(
                ProcessFactory,
                HttpClientFactory,
                LoggerFactory));

            _sentryHost = new Lazy<IDaprSentryHost>(() => new DaprSentryHost(
                ProcessFactory,
                HttpClientFactory,
                LoggerFactory));

            _sidecarHost = new Lazy<IDaprSidecarHost>(() => new DaprSidecarHost(
                ProcessFactory,
                HttpClientFactory,
                ApiTokenManager,
                LoggerFactory,
                SidecarInterceptors));
        }

        /// <summary>
        /// Gets the API Token Manager used for generating API tokens.
        /// </summary>
        public IDaprApiTokenManager ApiTokenManager { get; internal set; }

        /// <summary>
        /// Gets the Http Client Factor for accessing Dapr methods and service invocation.
        /// </summary>
        public IDaprSidecarHttpClientFactory HttpClientFactory { get; internal set; }

        /// <summary>
        /// Gets the Logger Factory for creating logger components.
        /// </summary>
        public IDaprLoggerFactory LoggerFactory { get; internal set; }

        /// <summary>
        /// Gets the Placement Host controller.
        /// </summary>
        public IDaprPlacementHost Placement => _placementHost.Value;

        /// <summary>
        /// Gets the process factory used to create Dapr processes.
        /// </summary>
        public IDaprProcessFactory ProcessFactory { get; internal set; }

        /// <summary>
        /// Gets the Sentry Host controller.
        /// </summary>
        public IDaprSentryHost Sentry => _sentryHost.Value;

        /// <summary>
        /// Gets the Sidecar Host controller.
        /// </summary>
        public IDaprSidecarHost Sidecar => _sidecarHost.Value;

        /// <summary>
        /// Gets the collection of interceptors for the Sidecar.
        /// </summary>
        public IEnumerable<IDaprSidecarProcessInterceptor> SidecarInterceptors { get; internal set; }
    }
}
