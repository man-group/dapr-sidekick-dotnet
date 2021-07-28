using System;
using System.IO;
using System.Reflection;

namespace Man.Dapr.Sidekick.Process
{
    internal class DaprSidecarProcess : DaprProcess<DaprSidecarOptions>, IDaprSidecarProcess
    {
        private const string AllowedOriginsArgument = "allowed-origins";
        private const string AppIdArgument = "app-id";
        private const string AppMaxConcurrencyArgument = "app-max-concurrency";
        private const string AppPortArgument = "app-port";
        private const string AppProtocolArgument = "app-protocol";
        private const string AppSslArgument = "app-ssl";
        private const string ComponentsPathArgument = "components-path";
        private const string ConfigFileArgument = "config";
        private const string ControlPlaneAddressArgument = "control-plane-address";
        private const string DaprGrpcPortArgument = "dapr-grpc-port";
        private const string DaprHttpMaxRequestSizeArgument = "dapr-http-max-request-size";
        private const string DaprHttpPortArgument = "dapr-http-port";
        private const string DaprInternalGrpcPortArgument = "dapr-internal-grpc-port";
        private const string EnableMtlsArgument = "enable-mtls";
        private const string EnableProfilingArgument = "enable-profiling";
        private const string KubeConfigArgument = "kubeconfig";
        private const string LogAsJsonArgument = "log-as-json";
        private const string LogLevelArgument = "log-level";
        private const string MetricsPortArgument = "metrics-port";
        private const string ModeArgument = "mode";
        private const string PlacementHostAddressArgument = "placement-host-address";
        private const string ProfilePortArgument = "profile-port";
        private const string SentryAddressArgument = "sentry-address";

        public DaprSidecarProcess()
            : base(DaprConstants.DaprSidecarProcessName)
        {
        }

        protected override DaprSidecarOptions GetProcessOptions(DaprOptions daprOptions)
        {
            // Get a clone of the current options as we will be modifying it.
            var options = daprOptions.Sidecar?.Clone() ?? new DaprSidecarOptions();
            options.EnrichFrom(daprOptions);

            // We have to have an AppId. If none already defined then set it to the assembly name
            if (string.IsNullOrEmpty(options.AppId))
            {
                // Default the AppId to name of the calling assembly, converting periods to hyphens and all text to lowercase.
                var executingAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
                options.AppId = executingAssembly.GetName().Name.Replace('.', '-').ToLower();
                Logger?.LogInformation("AppId not specified, assigning default value: {DaprSidecarAppId}", options.AppId);
            }

            // Set local placement information
            if (string.IsNullOrEmpty(options.PlacementHostAddress))
            {
                // If we have a local enabled placement running in this solution, then use that port
                // else use the defaults from the Dapr CLI - 6050 (Windows) or 50005 (Non-Windows)
                var port = daprOptions.Placement?.Enabled != false && daprOptions.Placement?.Port != null ? daprOptions.Placement?.Port.Value :
                    DaprConstants.IsWindows ? 6050 : 50005;
                options.PlacementHostAddress = $"{DaprConstants.LocalhostAddress}:{port}";
            }

            // Make sure we have a namespace
            options.Namespace ??= DaprConstants.DefaultNamespace;

            return options;
        }

        protected override void AssignPorts(PortAssignmentBuilder<DaprSidecarOptions> builder) =>
            builder
                .Add(x => x.AppPort, 8500)
                .Add(x => x.DaprGrpcPort, 50001)
                .Add(x => x.DaprHttpPort, 3500)
                .Add(x => x.MetricsPort, 9090)
                .Add(x => x.ProfilePort, 7777);

        protected override void AssignLocations(DaprSidecarOptions options, string daprFolder)
        {
            // Components directory - defaults to <daprFolder>\components
            var componentsDirectory = string.IsNullOrEmpty(options.ComponentsDirectory) ?
                Path.Combine(daprFolder, DaprConstants.DaprComponentsDirectory) :
                Path.GetFullPath(options.ComponentsDirectory);

            // Config file - defaults to <daprFolder>\config.yaml
            var configFile =
                string.IsNullOrEmpty(options.ConfigFile) ? Path.Combine(daprFolder, DaprConstants.DaprConfigFilename) :
                string.IsNullOrEmpty(Path.GetExtension(options.ConfigFile)) ? Path.GetFullPath(Path.Combine(options.ConfigFile, DaprConstants.DaprConfigFilename)) :
                Path.GetFullPath(options.ConfigFile);

            // Update the values
            options.ComponentsDirectory = componentsDirectory;
            options.ConfigFile = configFile;
        }

        protected override void AddCommandLineArguments(DaprSidecarOptions source, CommandLineArgumentBuilder builder) => builder
            .Add(AllowedOriginsArgument, source.AllowedOrigins)
            .Add(AppIdArgument, source.AppId)
            .Add(AppMaxConcurrencyArgument, source.AppMaxConcurrency)
            .Add(AppPortArgument, source.AppPort)
            .Add(AppProtocolArgument, source.AppProtocol)
            .Add(AppSslArgument, source.AppSsl)
            .Add(ControlPlaneAddressArgument, source.ControlPlaneAddress)
            .Add(DaprGrpcPortArgument, source.DaprGrpcPort)
            .Add(DaprHttpMaxRequestSizeArgument, source.DaprHttpMaxRequestSize)
            .Add(DaprHttpPortArgument, source.DaprHttpPort)
            .Add(DaprInternalGrpcPortArgument, source.DaprInternalGrpcPort)
            .Add(EnableMtlsArgument, source.Mtls)
            .Add(EnableProfilingArgument, source.Profiling)
            .Add(KubeConfigArgument, source.KubeConfig)
            .Add(LogAsJsonArgument, true) // All logging must be JSON
            .Add(LogLevelArgument, source.LogLevel)
            .Add(MetricsPortArgument, source.MetricsPort)
            .Add(ModeArgument, source.Mode)
            .Add(PlacementHostAddressArgument, source.PlacementHostAddress)
            .Add(ProfilePortArgument, source.ProfilePort, predicate: () => source.Profiling == true)
            .Add(SentryAddressArgument, source.SentryAddress, predicate: () => !source.SentryAddress.IsNullOrWhiteSpaceEx())
            .Add(ConfigFileArgument, source.ConfigFile, predicate: () => File.Exists(source.ConfigFile))
            .Add(ComponentsPathArgument, source.ComponentsDirectory, predicate: () => Directory.Exists(source.ComponentsDirectory))
            .Add(source.CustomArguments, requiresValue: false);

        protected override void AddEnvironmentVariables(DaprSidecarOptions source, EnvironmentVariableBuilder builder) => builder
            .Add(DaprConstants.AppApiTokenEnvironmentVariable, source.AppApiToken)
            .Add(DaprConstants.DaprApiTokenEnvironmentVariable, source.DaprApiToken)
            .Add(DaprConstants.DaprCertChainEnvironmentVariable, source.IssuerCertificate)
            .Add(DaprConstants.DaprCertKeyEnvironmentVariable, source.IssuerKey)
            .Add(DaprConstants.DaprGrpcPortEnvironmentVariable, source.DaprGrpcPort)
            .Add(DaprConstants.DaprHttpPortEnvironmentVariable, source.DaprHttpPort)
            .Add(DaprConstants.DaprProfilePortEnvironmentVariable, source.ProfilePort, () => source.Profiling == true)
            .Add(DaprConstants.DaprTrustAnchorsEnvironmentVariable, source.TrustAnchorsCertificate)
            .Add(DaprConstants.NamespaceEnvironmentVariable, source.Namespace, () => source.Mtls == true);

        protected override void ParseCommandLineArgument(DaprSidecarOptions target, string name, string value)
        {
            switch (name.ToLower())
            {
                case AllowedOriginsArgument:
                    target.AllowedOrigins = value;
                    break;

                case AppIdArgument:
                    target.AppId = value;
                    break;

                case AppMaxConcurrencyArgument:
                    target.AppMaxConcurrency = int.TryParse(value, out var appMaxConcurrency) ? (int?)appMaxConcurrency : null;
                    break;

                case AppPortArgument:
                    target.AppPort = int.TryParse(value, out var appPort) ? (int?)appPort : null;
                    break;

                case AppSslArgument:
                    target.AppSsl = !bool.TryParse(value, out var appSsl) || appSsl;
                    break;

                case AppProtocolArgument:
                    target.AppProtocol = value;
                    break;

                case ComponentsPathArgument:
                    target.ComponentsDirectory = value;
                    break;

                case ConfigFileArgument:
                    target.ConfigFile = value;
                    break;

                case ControlPlaneAddressArgument:
                    target.ControlPlaneAddress = value;
                    break;

                case DaprGrpcPortArgument:
                    target.DaprGrpcPort = int.TryParse(value, out var daprGrpcPort) ? (int?)daprGrpcPort : null;
                    break;

                case DaprHttpMaxRequestSizeArgument:
                    target.DaprHttpMaxRequestSize = int.TryParse(value, out var daprHttpMaxRequestSize) ? (int?)daprHttpMaxRequestSize : null;
                    break;

                case DaprHttpPortArgument:
                    target.DaprHttpPort = int.TryParse(value, out var daprHttpPort) ? (int?)daprHttpPort : null;
                    break;

                case DaprInternalGrpcPortArgument:
                    target.DaprInternalGrpcPort = int.TryParse(value, out var daprInternalGrpcPort) ? (int?)daprInternalGrpcPort : null;
                    break;

                case EnableMtlsArgument:
                    target.Mtls = !bool.TryParse(value, out var mtls) || mtls;
                    break;

                case KubeConfigArgument:
                    target.KubeConfig = value;
                    break;

                case LogLevelArgument:
                    target.LogLevel = value;
                    break;

                case MetricsPortArgument:
                    target.MetricsPort = int.TryParse(value, out var metricsPort) ? (int?)metricsPort : null;
                    break;

                case ModeArgument:
                    target.Mode = value;
                    break;

                case PlacementHostAddressArgument:
                    target.PlacementHostAddress = value;
                    break;

                case ProfilePortArgument:
                    target.ProfilePort = int.TryParse(value, out var profilePort) ? (int?)profilePort : null;
                    break;

                case SentryAddressArgument:
                    target.SentryAddress = value;
                    break;
            }
        }

        protected override ProcessComparison CompareProcessOptions(DaprSidecarOptions proposedProcessOptions, DaprSidecarOptions existingProcessOptions, IProcess existingProcess)
        {
            if (!string.Equals(proposedProcessOptions.AppId, existingProcessOptions.AppId, StringComparison.InvariantCultureIgnoreCase))
            {
                // Processes are for different app-ids
                return ProcessComparison.None;
            }
            else if (proposedProcessOptions.AppPort.HasValue && existingProcessOptions.AppPort.HasValue && proposedProcessOptions.AppPort == existingProcessOptions.AppPort)
            {
                // Same app-id, same app port. Attachable.
                Logger?.LogDebug(
                    "Found attachable process {DaprProcessName} PID:{DaprProcessId} with proposed app id {DaprSidecarAppId} and port {DaprSidecarAppPort}",
                    existingProcess.Name,
                    existingProcess.Id,
                    proposedProcessOptions.AppId,
                    proposedProcessOptions.AppPort);

                // As it is not possible to read the environment variables to obtain the API tokens, assume this is a development session and disable them if set.
                if (proposedProcessOptions.AppApiToken != null || proposedProcessOptions.UseDefaultAppApiToken != false)
                {
                    proposedProcessOptions.AppApiToken = null;
                    proposedProcessOptions.UseDefaultAppApiToken = false;
                    Logger?.LogDebug("Disabled AppApiToken as attaching to existing process");
                }

                if (proposedProcessOptions.DaprApiToken != null || proposedProcessOptions.UseDefaultDaprApiToken != false)
                {
                    proposedProcessOptions.DaprApiToken = null;
                    proposedProcessOptions.UseDefaultDaprApiToken = false;
                    Logger?.LogDebug("Disabled DaprApiToken as attaching to existing process");
                }

                return ProcessComparison.Attachable;
            }
            else
            {
                // Duplicate
                return ProcessComparison.Duplicate;
            }
        }
    }
}
