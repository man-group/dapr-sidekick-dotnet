using System;

namespace Man.Dapr.Sidekick
{
    public class DaprSentryOptions : Options.DaprProcessOptions
    {
        public DaprSentryOptions()
        {
            HealthPort = 8080;
        }

        /// <summary>
        /// Gets or sets the path to the Dapr configuration file. If a filename is not specified the default value "config.yaml" will be used.
        /// If a directory is not specified a directory called "dapr" under the current executing assembly location will be assumed.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Gets or sets custom arguments for the process. These will be appended "as-is" after all other arguments specified through these options.
        /// </summary>
        public string CustomArguments { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if Prometheus metrics are enabled in the service (default true).
        /// </summary>
        public bool? EnableMetrics { get; set; }

        /// <summary>
        /// Gets the HTTP port for the health server (default 8080).
        /// </summary>
        public int? HealthPort { get; internal set; } // Internal for now for testing, as cannot yet be set on sentry.exe

        /// <summary>
        /// Gets or sets the path to the credentials directory holding the issuer data.
        /// If not specified this will default to a directory called "certs" under the runtime folder.
        /// </summary>
        public string CertsDirectory { get; set; }

        /// <summary>
        /// Gets or sets the port for the metrics server (default "9092").
        /// </summary>
        public int? MetricsPort { get; set; }

        /// <summary>
        /// Gets or sets the CA trust domain (default "localhost").
        /// </summary>
        public string TrustDomain { get; set; }

        /// <summary>
        /// Creates a deep clone of this instance.
        /// </summary>
        /// <returns>A deep clone of this insteance.</returns>
        public new DaprSentryOptions Clone() => (DaprSentryOptions)base.Clone();

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
