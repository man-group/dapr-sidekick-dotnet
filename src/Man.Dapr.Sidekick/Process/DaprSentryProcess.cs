using System.IO;

namespace Man.Dapr.Sidekick.Process
{
    internal class DaprSentryProcess : DaprProcess<DaprSentryOptions>, IDaprSentryProcess
    {
        private const string ConfigFileArgument = "config";
        private const string EnableMetricsArgument = "enable-metrics";
        private const string IssuerCredentialsArgument = "issuer-credentials";
        private const string LogAsJsonArgument = "log-as-json";
        private const string LogLevelArgument = "log-level";
        private const string MetricsPortArgument = "metrics-port";
        private const string TrustDomainArgument = "trust-domain";

        public DaprSentryProcess()
            : base(DaprConstants.DaprSentryProcessName)
        {
        }

        protected override DaprSentryOptions GetProcessOptions(DaprOptions daprOptions)
        {
            // Get a clone of the current options as we will be modifying it.
            var options = daprOptions.Sentry?.Clone() ?? new DaprSentryOptions();
            options.EnrichFrom(daprOptions);

            // The Sentry service needs to maintain the same ports on restarts to ensure if either
            // it or the sidecar are restarted out of sequence the port allocations remain the same.
            // This should be fine in general as the Sentry service normally runs on a dedicated host.
            options.RetainPortsOnRestart = true;

            return options;
        }

        protected override void AssignPorts(PortAssignmentBuilder<DaprSentryOptions> builder) =>
            builder
                .Add(x => x.MetricsPort, 9092);

        // Can only have one instance of Sentry on a single machine
        protected override void AssignLocations(DaprSentryOptions options, string daprFolder)
        {
            // Certs directory - defaults to <daprFolder>\certs
            var certsDirectory = string.IsNullOrEmpty(options.CertsDirectory) ?
                Path.Combine(daprFolder, DaprConstants.DaprCertsDirectory) :
                Path.GetFullPath(options.CertsDirectory);

            // Config file - defaults to <daprFolder>\config.yaml
            var configFile =
                string.IsNullOrEmpty(options.ConfigFile) ? Path.Combine(daprFolder, DaprConstants.DaprConfigFilename) :
                string.IsNullOrEmpty(Path.GetExtension(options.ConfigFile)) ? Path.GetFullPath(Path.Combine(options.ConfigFile, DaprConstants.DaprConfigFilename)) :
                Path.GetFullPath(options.ConfigFile);

            // Write any defined certificate options
            WriteDefaultCertificates(certsDirectory, options);

            // Update the values
            options.CertsDirectory = certsDirectory;
            options.ConfigFile = configFile;
        }

        protected override void AddCommandLineArguments(DaprSentryOptions source, CommandLineArgumentBuilder builder) => builder
            .Add(EnableMetricsArgument, source.EnableMetrics)
            .Add(IssuerCredentialsArgument, source.CertsDirectory)
            .Add(LogAsJsonArgument, true) // All logging must be JSON
            .Add(LogLevelArgument, source.LogLevel)
            .Add(MetricsPortArgument, source.MetricsPort)
            .Add(TrustDomainArgument, source.TrustDomain)
            .Add(ConfigFileArgument, source.ConfigFile, predicate: () => File.Exists(source.ConfigFile))
            .Add(source.CustomArguments, requiresValue: false);

        protected override void AddEnvironmentVariables(DaprSentryOptions source, EnvironmentVariableBuilder builder) => builder
            .Add(DaprConstants.DaprSentryMetricsPortEnvironmentVariable, source.MetricsPort, () => source.EnableMetrics != false);

        protected override void ParseCommandLineArgument(DaprSentryOptions target, string name, string value)
        {
            switch (name.ToLower())
            {
                case ConfigFileArgument:
                    target.ConfigFile = value;
                    break;

                case EnableMetricsArgument:
                    target.EnableMetrics = !bool.TryParse(value, out var enableMetrics) || enableMetrics;
                    break;

                case IssuerCredentialsArgument:
                    target.CertsDirectory = value;
                    break;

                case LogLevelArgument:
                    target.LogLevel = value;
                    break;

                case MetricsPortArgument:
                    target.MetricsPort = int.TryParse(value, out var metricsPort) ? (int?)metricsPort : null;
                    break;

                case TrustDomainArgument:
                    target.TrustDomain = value;
                    break;
            }
        }

        protected override ProcessComparison CompareProcessOptions(DaprSentryOptions proposedProcessOptions, DaprSentryOptions existingProcessOptions, IProcess existingProcess) => ProcessComparison.Duplicate;
    }
}
