using System;
using System.IO;

namespace Dapr.Sidekick.Process
{
    internal class DaprPlacementProcess : DaprProcess<DaprPlacementOptions>, IDaprPlacementProcess
    {
        private const string CertChainArgument = "certchain";
        private const string EnableMetricsArgument = "enable-metrics";
        private const string HealthzPortArgument = "healthz-port";
        private const string IdArgument = "id";
        private const string InitialClusterArgument = "initial-cluster";
        private const string InmemStoreEnabledArgument = "inmem-store-enabled";
        private const string LogAsJsonArgument = "log-as-json";
        private const string LogLevelArgument = "log-level";
        private const string MetricsPortArgument = "metrics-port";
        private const string PortArgument = "port";
        private const string RaftLogstorePathArgument = "raft-logstore-path";
        private const string ReplicationFactorArgument = "replicationfactor";
        private const string TlsEnabledArgument = "tls-enabled";

        public DaprPlacementProcess()
            : base(DaprConstants.DaprPlacementProcessName)
        {
        }

        protected override DaprPlacementOptions GetProcessOptions(DaprOptions daprOptions)
        {
            // Get a clone of the current options as we will be modifying it.
            var options = daprOptions.Placement?.Clone() ?? new DaprPlacementOptions();
            options.EnrichFrom(daprOptions);

            // The placement service needs to maintain the same ports on restarts to ensure if either
            // it or the sidecar are restarted out of sequence the port allocations remain the same.
            // This should be fine in general as the placement service normally runs on a dedicated host.
            options.RetainPortsOnRestart = true;

            return options;
        }

        protected override void AssignPorts(PortAssignmentBuilder<DaprPlacementOptions> builder) =>
            builder
                .Add(x => x.HealthPort, 8081)
                .Add(x => x.MetricsPort, 9091)
                .Add(x => x.Port, 50005);

        protected override void AssignLocations(DaprPlacementOptions options, string daprFolder)
        {
            // Certs directory - defaults to <daprFolder>\certs
            var certsDirectory = string.IsNullOrEmpty(options.CertsDirectory) ?
                Path.Combine(daprFolder, DaprConstants.DaprCertsDirectory) :
                Path.GetFullPath(options.CertsDirectory);

            // Write any defined certificate options
            WriteDefaultCertificates(certsDirectory, options);

            // Update the values
            options.CertsDirectory = certsDirectory;
        }

        protected override void AddCommandLineArguments(DaprPlacementOptions source, CommandLineArgumentBuilder builder) => builder
            .Add(CertChainArgument, source.CertsDirectory)
            .Add(EnableMetricsArgument, source.EnableMetrics)
            .Add(HealthzPortArgument, source.HealthPort)
            .Add(IdArgument, source.Id)
            .Add(InitialClusterArgument, source.InitialCluster)
            .Add(InmemStoreEnabledArgument, source.InmemStoreEnabled)
            .Add(LogAsJsonArgument, true) // All logging must be JSON
            .Add(LogLevelArgument, source.LogLevel)
            .Add(MetricsPortArgument, source.MetricsPort)
            .Add(PortArgument, source.Port)
            .Add(RaftLogstorePathArgument, source.RaftLogstorePath)
            .Add(ReplicationFactorArgument, source.ReplicationFactor)
            .Add(TlsEnabledArgument, source.Mtls)
            .Add(source.CustomArguments, requiresValue: false);

        protected override void AddEnvironmentVariables(DaprPlacementOptions source, EnvironmentVariableBuilder builder) => builder
            .Add(DaprConstants.DaprPlacementPortEnvironmentVariable, source.Port)
            .Add(DaprConstants.DaprPlacementHealthPortEnvironmentVariable, source.HealthPort)
            .Add(DaprConstants.DaprPlacementMetricsPortEnvironmentVariable, source.MetricsPort, () => source.EnableMetrics != false);

        protected override void ParseCommandLineArgument(DaprPlacementOptions target, string name, string value)
        {
            switch (name.ToLower())
            {
                case CertChainArgument:
                    target.CertsDirectory = value;
                    break;

                case EnableMetricsArgument:
                    target.EnableMetrics = !bool.TryParse(value, out var enableMetrics) || enableMetrics;
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

                case InmemStoreEnabledArgument:
                    target.InmemStoreEnabled = !bool.TryParse(value, out var inmemStoreEnabled) || inmemStoreEnabled;
                    break;

                case LogLevelArgument:
                    target.LogLevel = value;
                    break;

                case MetricsPortArgument:
                    target.MetricsPort = int.TryParse(value, out var metricsPort) ? (int?)metricsPort : null;
                    break;

                case PortArgument:
                    target.Port = int.TryParse(value, out var port) ? (int?)port : null;
                    break;

                case RaftLogstorePathArgument:
                    target.RaftLogstorePath = value;
                    break;

                case ReplicationFactorArgument:
                    target.ReplicationFactor = int.TryParse(value, out var replicationFactor) ? (int?)replicationFactor : null;
                    break;

                case TlsEnabledArgument:
                    target.Mtls = !bool.TryParse(value, out var tlsEnabled) || tlsEnabled;
                    break;
            }
        }

        protected override ProcessComparison CompareProcessOptions(DaprPlacementOptions proposedProcessOptions, DaprPlacementOptions existingProcessOptions, IProcess existingProcess)
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
                    "Found attachable process {DaprProcessName} PID:{DaprProcessId} with proposed id {DaprPlacementId} and port {DaprPlacementPort}",
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
