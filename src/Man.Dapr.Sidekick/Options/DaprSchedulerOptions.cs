using System;

namespace Man.Dapr.Sidekick
{
    public class DaprSchedulerOptions : Options.DaprProcessOptions
    {
        /// <summary>
        /// Gets or sets the path to the credentials directory holding the issuer data.
        /// If not specified this will default to a directory called "certs" under the runtime folder.
        /// Provided for backwards compatibility logic only.
        /// </summary>
        public string CertsDirectory { get; set; }

        /// <summary>
        /// Gets or sets custom arguments for the process. These will be appended "as-is" after all other arguments specified through these options.
        /// </summary>
        public string CustomArguments { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if Prometheus metrics are enabled in the scheduler service (default true).
        /// </summary>
        public bool? EnableMetrics { get; set; }

        /// <summary>
        /// Gets or sets the ports for etcd client http communication.
        /// </summary>
        public string EtcdClientHttpPorts { get; set; }

        /// <summary>
        /// Gets or sets the ports for etcd client communication (default [dapr-scheduler-server-0=2379]).
        /// </summary>
        public string EtcdClientPorts { get; set; }

        /// <summary>
        /// Gets or sets the compaction mode for etcd. Can be 'periodic' or 'revision' (default "periodic").
        /// </summary>
        public string EtcdCompactionMode { get; set; }

        /// <summary>
        /// Gets or sets the compaction retention for etcd.
        /// Can express time or number of revisions, depending on the value of <see cref="EtcdCompactionMode"/> (default "24h").
        /// </summary>
        public string EtcdCompactionRetention { get; set; }

        /// <summary>
        /// Gets or sets the directory to store scheduler etcd data (default "./data").
        /// </summary>
        public string EtcdDataDir { get; set; }

        /// <summary>
        /// Gets or sets the space quota for etcd (default 2147483648).
        /// </summary>
        public int? EtcdSpaceQuota { get; set; }

        /// <summary>
        /// Gets or sets the listening address for the healthz server.
        /// </summary>
        public string HealthListenAddress { get; set; }

        /// <summary>
        /// Gets or sets the HTTP port for the health server (default 8082).
        /// </summary>
        public int? HealthPort { get; set; }

        /// <summary>
        /// Gets or sets the scheduler server ID (default "dapr-scheduler-server-0").
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the raft cluster peers (default "dapr-scheduler-server-0=http://localhost:2380").
        /// </summary>
        public string InitialCluster { get; set; }

        /// <summary>
        /// Gets or sets the address for the scheduler server to listen on.
        /// </summary>
        public string ListenAddress { get; set; }

        /// <summary>
        /// Gets or sets the address for the metrics server (default "0.0.0.0").
        /// </summary>
        public string MetricsListenAddress { get; set; }

        /// <summary>
        /// Gets or sets the port for the metrics server (default 9093).
        /// </summary>
        public int? MetricsPort { get; set; }

        /// <summary>
        /// Gets or sets the runtime mode for the scheduler service (default "standalone").
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets the gRPC port for the scheduler service (defaults to 6060 on Windows and 50006 on other platforms).
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the total number of scheduler replicas in the cluster (default 1).
        /// </summary>
        public uint? ReplicaCount { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if TLS should be enabled for the scheduler gRPC server.
        /// </summary>
        public bool? TlsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the filepath to the trust anchors for the Dapr control plane (default "/var/run/secrets/dapr.io/tls/ca.crt").
        /// </summary>
        public string TrustAnchorsFile { get; set; }

        /// <summary>
        /// Gets or sets the trust domain for the Dapr control plane (default "localhost").
        /// </summary>
        public string TrustDomain { get; set; }

        /// <summary>
        /// Creates a deep clone of this instance.
        /// </summary>
        /// <returns>A deep clone of this insteance.</returns>
        public new DaprSchedulerOptions Clone() => (DaprSchedulerOptions)base.Clone();

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
