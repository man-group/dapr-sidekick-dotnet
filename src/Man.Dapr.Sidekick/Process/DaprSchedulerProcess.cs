using System;
using System.IO;

namespace Man.Dapr.Sidekick.Process
{
    internal class DaprSchedulerProcess : DaprProcess<DaprSchedulerOptions>, IDaprSchedulerProcess
    {
        private const string EnableMetricsArgument = "enable-metrics";
        private const string EtcdClientHttpPortsArgument = "etcd-client-http-ports";
        private const string EtcdClientPortsArgument = "etcd-client-ports";
        private const string EtcdCompactionModeArgument = "etcd-compaction-mode";
        private const string EtcdCompactionRetentionArgument = "etcd-compaction-retention";
        private const string EtcdDataDirArgument = "etcd-data-dir";
        private const string EtcdSpaceQuotaArgument = "etcd-space-quota";
        private const string HealthzListenAddressArgument = "healthz-listen-address";
        private const string HealthzPortArgument = "healthz-port";
        private const string IdArgument = "id";
        private const string InitialClusterArgument = "initial-cluster";
        private const string ListenAddressArgument = "listen-address";
        private const string LogAsJsonArgument = "log-as-json";
        private const string LogLevelArgument = "log-level";
        private const string MetricsListenAddressArgument = "metrics-listen-address";
        private const string MetricsPortArgument = "metrics-port";
        private const string ModeArgument = "mode";
        private const string PortArgument = "port";
        private const string ReplicaCountArgument = "replica-count";
        private const string TlsEnabledArgument = "tls-enabled";
        private const string TrustAnchorsFileArgument = "trust-anchors-file";
        private const string TrustDomainArgument = "trust-domain";

        public DaprSchedulerProcess()
            : base(DaprConstants.DaprSchedulerProcessName)
        {
        }

        protected override DaprSchedulerOptions GetProcessOptions(DaprOptions daprOptions)
        {
            // Get a clone of the current options as we will be modifying it.
            var options = daprOptions.Scheduler?.Clone() ?? new DaprSchedulerOptions();
            options.EnrichFrom(daprOptions);

            // The scheduler service needs to maintain the same ports on restarts to ensure if either
            // it or the sidecar are restarted out of sequence the port allocations remain the same.
            // This should be fine in general as the scheduler service normally runs on a dedicated host.
            options.RetainPortsOnRestart = true;

            return options;
        }

        protected override void AssignPorts(PortAssignmentBuilder<DaprSchedulerOptions> builder) =>
            builder
                .Add(x => x.HealthPort, 8082)
                .Add(x => x.MetricsPort, 9093)
                .Add(x => x.Port, DaprConstants.IsWindows ? 6060 : 50006);

        protected override void AssignLocations(DaprSchedulerOptions options, string daprFolder)
        {
            // Certs directory - defaults to <daprFolder>\certs
            var certsDirectory = string.IsNullOrEmpty(options.CertsDirectory) ?
                Path.Combine(daprFolder, DaprConstants.DaprCertsDirectory) :
                Path.GetFullPath(options.CertsDirectory);

            // Write any defined certificate options
            WriteDefaultCertificates(certsDirectory, options);

            // Pass the trust anchors file to scheduler
            options.TrustAnchorsFile ??= Path.Combine(certsDirectory, DaprConstants.TrustAnchorsCertificateFilename);
        }

        protected override void AddCommandLineArguments(DaprSchedulerOptions source, CommandLineArgumentBuilder builder) => builder
            .Add(EnableMetricsArgument, source.EnableMetrics)
            .Add(EtcdClientHttpPortsArgument, source.EtcdClientHttpPorts)
            .Add(EtcdClientPortsArgument, source.EtcdClientPorts)
            .Add(EtcdCompactionModeArgument, source.EtcdCompactionMode)
            .Add(EtcdCompactionRetentionArgument, source.EtcdCompactionRetention)
            .Add(EtcdDataDirArgument, source.EtcdDataDir)
            .Add(EtcdSpaceQuotaArgument, source.EtcdSpaceQuota)
            .Add(HealthzListenAddressArgument, source.HealthListenAddress)
            .Add(HealthzPortArgument, source.HealthPort)
            .Add(IdArgument, source.Id)
            .Add(InitialClusterArgument, source.InitialCluster)
            .Add(ListenAddressArgument, source.ListenAddress)
            .Add(LogAsJsonArgument, true) // All logging must be JSON
            .Add(LogLevelArgument, source.LogLevel)
            .Add(MetricsListenAddressArgument, source.MetricsListenAddress)
            .Add(MetricsPortArgument, source.MetricsPort)
            .Add(ModeArgument, source.Mode)
            .Add(PortArgument, source.Port)
            .Add(ReplicaCountArgument, source.ReplicaCount)
            .Add(TlsEnabledArgument, source.TlsEnabled)
            .Add(TrustAnchorsFileArgument, source.TrustAnchorsFile)
            .Add(TrustDomainArgument, source.TrustDomain)
            .Add(source.CustomArguments, requiresValue: false);

        protected override void AddEnvironmentVariables(DaprSchedulerOptions source, EnvironmentVariableBuilder builder) => builder
            .Add(DaprConstants.DaprSchedulerPortEnvironmentVariable, source.Port)
            .Add(DaprConstants.DaprSchedulerHealthPortEnvironmentVariable, source.HealthPort)
            .Add(DaprConstants.DaprSchedulerMetricsPortEnvironmentVariable, source.MetricsPort, () => source.EnableMetrics != false);

        protected override void ParseCommandLineArgument(DaprSchedulerOptions target, string name, string value)
        {
            switch (name.ToLower())
            {
                case EnableMetricsArgument:
                    target.EnableMetrics = !bool.TryParse(value, out var enableMetrics) || enableMetrics;
                    break;

                case EtcdClientHttpPortsArgument:
                    target.EtcdClientHttpPorts = value;
                    break;

                case EtcdClientPortsArgument:
                    target.EtcdClientPorts = value;
                    break;

                case EtcdCompactionModeArgument:
                    target.EtcdCompactionMode = value;
                    break;

                case EtcdCompactionRetentionArgument:
                    target.EtcdCompactionRetention = value;
                    break;

                case EtcdDataDirArgument:
                    target.EtcdDataDir = value;
                    break;

                case EtcdSpaceQuotaArgument:
                    target.EtcdSpaceQuota = int.TryParse(value, out var etcdSpaceQuota) ? (int?)etcdSpaceQuota : null;
                    break;

                case HealthzListenAddressArgument:
                    target.HealthListenAddress = value;
                    break;

                case HealthzPortArgument:
                    target.HealthPort = int.TryParse(value, out var healthzPort) ? (int?)healthzPort : null;
                    break;

                case IdArgument:
                    target.Id = value;
                    break;

                case InitialClusterArgument:
                    target.InitialCluster = value;
                    break;

                case ListenAddressArgument:
                    target.ListenAddress = value;
                    break;

                case LogLevelArgument:
                    target.LogLevel = value;
                    break;

                case MetricsListenAddressArgument:
                    target.MetricsListenAddress = value;
                    break;

                case MetricsPortArgument:
                    target.MetricsPort = int.TryParse(value, out var metricsPort) ? (int?)metricsPort : null;
                    break;

                case ModeArgument:
                    target.Mode = value;
                    break;

                case PortArgument:
                    target.Port = int.TryParse(value, out var port) ? (int?)port : null;
                    break;

                case ReplicaCountArgument:
                    target.ReplicaCount = uint.TryParse(value, out var replicaCount) ? (uint?)replicaCount : null;
                    break;

                case TlsEnabledArgument:
                    target.TlsEnabled = !bool.TryParse(value, out var tlsEnabled) || tlsEnabled;
                    break;

                case TrustAnchorsFileArgument:
                    target.TrustAnchorsFile = value;
                    break;

                case TrustDomainArgument:
                    target.TrustDomain = value;
                    break;
            }
        }

        protected override ProcessComparison CompareProcessOptions(DaprSchedulerOptions proposedProcessOptions, DaprSchedulerOptions existingProcessOptions, IProcess existingProcess)
        {
            if (!string.Equals(proposedProcessOptions.Id, existingProcessOptions.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                // Processes are for different app-ids
                return ProcessComparison.None;
            }
            else if (proposedProcessOptions.Port.HasValue && existingProcessOptions.Port.HasValue && proposedProcessOptions.Port == existingProcessOptions.Port)
            {
                // Same app-id, same app port. Attachable.
                Logger?.LogDebug(
                    "Found attachable process {DaprProcessName} PID:{DaprProcessId} with proposed id {DaprSchedulerId} and port {DaprSchedulerPort}",
                    existingProcess.Name,
                    existingProcess.Id,
                    proposedProcessOptions.Id,
                    proposedProcessOptions.Port);
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
