using System;

namespace Dapr.Sidekick
{
    public class DaprPlacementOptions : Options.DaprProcessOptions
    {
        /// <summary>
        /// Gets or sets the path to the credentials directory holding the issuer data.
        /// If not specified this will default to a directory called "certs" under the runtime folder.
        /// </summary>
        public string CertsDirectory { get; set; }

        /// <summary>
        /// Gets or sets custom arguments for the process. These will be appended "as-is" after all other arguments specified through these options.
        /// </summary>
        public string CustomArguments { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if Prometheus metrics are enabled in the Placement service (default true).
        /// </summary>
        public bool? EnableMetrics { get; set; }

        /// <summary>
        /// Gets or sets the HTTP port for the health server (default 8081).
        /// </summary>
        public int? HealthPort { get; set; }

        /// <summary>
        /// Gets or sets the placement server ID (default "dapr-placement-0").
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the raft cluster peers (default "dapr-placement-0=127.0.0.1:8201").
        /// </summary>
        public string InitialCluster { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if in-memory log and snapshot store is enabled, unless raft-logstore-path is set (default true).
        /// </summary>
        public bool? InmemStoreEnabled { get; set; }

        /// <summary>
        /// Gets or sets the port for the metrics server (default 9091).
        /// </summary>
        public int? MetricsPort { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if TLS should be enabled for the placement gRPC server.
        /// </summary>
        public bool? Mtls { get; set; }

        /// <summary>
        /// Gets or sets the gRPC port for the placement service (default 50005).
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the path to the raft log store.
        /// </summary>
        public string RaftLogstorePath { get; set; }

        /// <summary>
        /// Gets or sets the replication factor for actor distribution on vnodes (default 100).
        /// </summary>
        public int? ReplicationFactor { get; set; }

        /// <summary>
        /// Creates a deep clone of this instance.
        /// </summary>
        /// <returns>A deep clone of this insteance.</returns>
        public new DaprPlacementOptions Clone() => (DaprPlacementOptions)base.Clone();

        protected override bool AddHealthUri(UriBuilder builder)
        {
            if (!HealthPort.HasValue)
            {
                return false;
            }

            builder.Port = HealthPort.Value;
            builder.Path = "healthz";
            return true;
        }

        protected override bool AddMetricsUri(UriBuilder builder)
        {
            if (!MetricsPort.HasValue || EnableMetrics == false)
            {
                return false;
            }

            builder.Port = MetricsPort.Value;
            return true;
        }
    }
}
