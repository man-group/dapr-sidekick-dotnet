using System;
using Dapr.Sidekick.Security;

namespace Dapr.Sidekick
{
    public class DaprSidecarOptions : Options.DaprProcessOptions
    {
        public DaprSidecarOptions()
        {
            // Set the default shutdown wait time to be 10 seconds to give the sidecar a chance to respond the "shutdown" command.
            WaitForShutdownSeconds = 10;
        }

        /// <summary>
        /// Gets or sets the allowed HTTP origins (default "*").
        /// </summary>
        public string AllowedOrigins { get; set; }

        /// <summary>
        /// Gets or sets the API Token that Dapr will provide as a header in each request sent to an application from the sidecar.
        /// When specified the Dapr process will be launched with the environment variable APP_API_TOKEN set to this value.
        /// See https://docs.dapr.io/operations/security/app-api-token/.
        /// </summary>
        public SensitiveString AppApiToken { get; set; }

        /// <summary>
        /// Gets or sets a unique ID for Dapr. Used for Service Discovery and state.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets a value that controls the concurrency level when forwarding requests to user code (default -1).
        /// </summary>
        public int? AppMaxConcurrency { get; set; }

        /// <summary>
        /// Gets or sets the port the application is listening on.
        /// </summary>
        public int? AppPort { get; set; }

        /// <summary>
        /// Gets or sets the callback protocol for the application: grpc or http (default "http").
        /// </summary>
        public string AppProtocol { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if the sidecar should set the URI scheme of the app to https and attempts an SSL connection. Defaults to <c>false</c>.
        /// </summary>
        public bool? AppSsl { get; set; }

        /// <summary>
        /// Gets or sets the path to the Dapr Components directory containing component configurations.
        /// If not specified this will default to a directory called "components" under the current dapr folder.
        /// </summary>
        public string ComponentsDirectory { get; set; }

        /// <summary>
        /// Gets or sets the path to the Dapr configuration file. If a filename is not specified the default value "config.yaml" will be used.
        /// If not specified this will default to the "config.yaml" file in the current dapr folder.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets the address for a Dapr control plane.
        /// </summary>
        public string ControlPlaneAddress { get; set; }

        /// <summary>
        /// Gets or sets custom arguments for the process. These will be appended "as-is" after all other arguments specified through these options.
        /// </summary>
        public string CustomArguments { get; set; }

        /// <summary>
        /// Gets or sets the API Token that Dapr will expect to see in the header of each public API request to authenticate the caller.
        /// When specified the Dapr process will be launched with the environment variable DAPR_API_TOKEN set to this value.
        /// See https://docs.dapr.io/operations/security/api-token/.
        /// </summary>
        public SensitiveString DaprApiToken { get; set; }

        /// <summary>
        /// Gets or sets the gRPC port for the Dapr API to listen on (default "50001").
        /// </summary>
        public int? DaprGrpcPort { get; set; }

        /// <summary>
        /// Gets or sets the max size of request body in MB to handle uploading of big files (default 4 MB).
        /// </summary>
        public int? DaprHttpMaxRequestSize { get; set; }

        /// <summary>
        /// Gets or sets the HTTP port for Dapr API to listen on (default "3500").
        /// </summary>
        public int? DaprHttpPort { get; set; }

        /// <summary>
        /// Gets or sets the gRPC port for the Dapr Internal API to listen on.
        /// </summary>
        public int? DaprInternalGrpcPort { get; set; }

        /// <summary>
        /// Gets or sets the absolute path to the kubeconfig file (default %USERPROFILE%/.kube/config).
        /// </summary>
        public string KubeConfig { get; set; }

        /// <summary>
        /// Gets or sets the port for the metrics server (default "9090").
        /// </summary>
        public int? MetricsPort { get; set; }

        /// <summary>
        /// Gets or sets the runtime mode for the Dapr sidecar (default "standalone").
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if automatic mTLS is enabled for sidecar to sidecar communication channels. Defaults to <c>false</c>.
        /// </summary>
        public bool? Mtls { get; set; }

        /// <summary>
        /// Gets or sets the namespace for this sidecar instance.
        /// The Dapr process will be launched with the environment variable NAMESPACE set to this value.
        /// Defaults to "default" if not specified.
        /// </summary>
        /// <remarks>
        /// Dapr uses namespaces to determine which services can call other services. For example, components in the
        /// "development" namespace cannot call components in the "production" namespace.
        /// The NAMESPACE environment variable MUST be set when <see cref="Mtls"/> is enabled as it is encoded
        /// into the SPIFFE identifier.
        /// </remarks>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the actor placement service host address. This is typically a comma-separated list of host:port endpoints.
        /// </summary>
        public string PlacementHostAddress { get; set; }

        /// <summary>
        /// Gets or sets the port for the profile server (default "7777").
        /// </summary>
        public int? ProfilePort { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if Profiling is enabled in the Dapr Sidecar. Defaults to <c>false</c>.
        /// </summary>
        public bool? Profiling { get; set; }

        /// <summary>
        /// Gets or sets the address for the Sentry CA service.
        /// </summary>
        public string SentryAddress { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if a default API token is generated if <see cref="AppApiToken"/> is not specified. Defaults to <c>false</c>.
        /// </summary>
        public bool? UseDefaultAppApiToken { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if a default API token is generated if <see cref="DaprApiToken"/> is not specified. Defaults to <c>false</c>.
        /// </summary>
        public bool? UseDefaultDaprApiToken { get; set; }

        /// <summary>
        /// Gets the address of the metdata endpoint, such as http://127.0.0.1:3500/v1.0/metadata.
        /// </summary>
        /// <returns>The metadata endpoint address.</returns>
        public Uri GetMetadataUri() => GetLocalUri(builder => AddMetadataUri(builder));

        /// <summary>
        /// Gets the address of the shutdown endpoint, such as http://127.0.0.1:3500/v1.0/shutdown.
        /// </summary>
        /// <returns>The metadata endpoint address.</returns>
        public Uri GetShutdownUri() => GetLocalUri(builder => AddShutdownUri(builder));

        /// <summary>
        /// Creates a deep clone of this instance.
        /// </summary>
        /// <returns>A deep clone of this insteance.</returns>
        public new DaprSidecarOptions Clone() => (DaprSidecarOptions)base.Clone();

        protected override bool AddHealthUri(UriBuilder builder)
        {
            if (!DaprHttpPort.HasValue)
            {
                return false;
            }

            builder.Port = DaprHttpPort.Value;
            builder.Path = "v1.0/healthz";
            return true;
        }

        protected override bool AddMetricsUri(UriBuilder builder)
        {
            if (!MetricsPort.HasValue)
            {
                return false;
            }

            builder.Port = MetricsPort.Value;
            return true;
        }

        private bool AddMetadataUri(UriBuilder builder)
        {
            if (!DaprHttpPort.HasValue)
            {
                return false;
            }

            builder.Port = DaprHttpPort.Value;
            builder.Path = "v1.0/metadata";
            return true;
        }

        private bool AddShutdownUri(UriBuilder builder)
        {
            if (!DaprHttpPort.HasValue)
            {
                return false;
            }

            builder.Port = DaprHttpPort.Value;
            builder.Path = "v1.0/shutdown";
            return true;
        }
    }
}
