using System;
using System.Collections.Generic;
using System.Linq;
using Man.Dapr.Sidekick.Security;

namespace Man.Dapr.Sidekick.Options
{
    public abstract class DaprProcessOptions
    {
        /// <summary>
        /// Gets or sets the full path to the directory containing the process binary.
        /// If specified and the binary cannot be found in this directory and <see cref="CopyProcessFile"/> is <c>true</c>, the process binary
        /// will be copied to this directory from <see cref="RuntimeDirectory"/>/bin.
        /// Defaults to <see cref="RuntimeDirectory"/>/bin if not specified.
        /// </summary>
        public string BinDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if the Dapr process binary is copied to <see cref="BinDirectory"/> from <see cref="InitialDirectory"/> when different.
        /// This allows new versions of binaries to be deployed to <see cref="InitialDirectory"/> while an application is running, such that on restart the application picks up the new version.
        /// Default is <c>false</c>.
        /// </summary>
        public bool? CopyProcessFile { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if the Dapr process is enabled. If <c>false</c> the Dapr process binary will not be launched or managed on startup.
        /// This allows an application to use Dapr Sidekick for development but leverage standard Dapr runtime mechanisms for deployments.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// Gets or sets the full path to the initial directory containing the Dapr components.
        /// Typically this is the directory created by the "dapr init" command containing
        /// the config.yaml file and the bin, components and certs subdirectories.
        /// Defaults to %USERPROFILE%/.dapr ($HOME/.dapr on Linux) if not specified.
        /// </summary>
        public string InitialDirectory { get; set; }

        /// <summary>
        /// Gets or sets a working directory to be used by the managed process.
        /// This is required by some components that use relevant directory paths to load local files from the project folders.
        /// Defaults to %USERPROFILE%/.dapr ($HOME/.dapr on Linux) if not specified.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the issuer certificate used for mTLS encryption.
        /// </summary>
        public SensitiveString IssuerCertificate { get; set; }

        /// <summary>
        /// Gets or sets the issuer private key used for mTLS certificates.
        /// </summary>
        public SensitiveString IssuerKey { get; set; }

        /// <summary>
        /// Gets or sets the log level. Options are debug, info, warning, error, or fatal (default "info").
        /// </summary>
        public string LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the options for managing and encriching the metrics exposed by the Dapr binary.
        /// </summary>
        public DaprMetricsOptions Metrics { get; set; }

        /// <summary>
        /// Gets or sets the full path to the filename of the Dapr process binary.
        /// Defaults to <see cref="BinDirectory"/>/<see cref="ProcessName"/>.exe (<see cref="BinDirectory"/>/<see cref="ProcessName"/> on Linux) if not specified.
        /// </summary>
        public string ProcessFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the process used to detemine <see cref="ProcessFile"/> and identify duplicate/attachable instances.
        /// If not specified the appropriate default value for the process will be used.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// Gets or sets the interval after which a restart will be attempted for the Dapr sidecar should it fail to start up or exit unexpectedly.
        /// Set this to any negative number to prevent restart attempts. Defaults to 5000 (5 seconds) if not defined.
        /// </summary>
        public int? RestartAfterMillseconds { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if any automatically assigned ports are retained on restart.
        /// If <c>false</c> the port assignments will be updated to use the next available set of ports as reported by the operating system.
        /// Defaults to <c>true</c> if not specified to ensure existing port assignments are retained on restart.
        /// </summary>
        public bool? RetainPortsOnRestart { get; set; }

        /// <summary>
        /// Gets or sets the full path to the runtime directory of the Dapr components.
        /// Defaults to <see cref="InitialDirectory"/> if not specified.
        /// </summary>
        public string RuntimeDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value that determines the number of seconds to wait for the process to shut down gracefully before it is force killed.
        /// If not specified the process will be force killed immediately.
        /// </summary>
        public int? WaitForShutdownSeconds { get; set; }

        /// <summary>
        /// Gets or sets the trust anchor certificate used for mTLS encryption.
        /// </summary>
        public SensitiveString TrustAnchorsCertificate { get; set; }

        /// <summary>
        /// Gets or sets a set of Environment Variables to be set when the process is started.
        /// These will override any other environment variables calculated from other configuration settings.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; }

        /// <summary>
        /// Creates a deep clone of this instance.
        /// </summary>
        /// <returns>A deep clone of this insteance.</returns>
        public DaprProcessOptions Clone()
        {
            var clone = (DaprProcessOptions)MemberwiseClone();
            clone.Metrics = Metrics?.Clone();
            return clone;
        }

        /// <summary>
        /// Updates any undefined properties in this instance with the values in <paramref name="source"/> where specified.
        /// </summary>
        /// <param name="source">A source options instance.</param>
        public void EnrichFrom(DaprProcessOptions source)
        {
            if (source == null)
            {
                return;
            }

            BinDirectory ??= source.BinDirectory;
            CopyProcessFile ??= source.CopyProcessFile;
            Enabled ??= source.Enabled;
            InitialDirectory ??= source.InitialDirectory;
            IssuerCertificate ??= source.IssuerCertificate;
            IssuerKey ??= source.IssuerKey;
            LogLevel ??= source.LogLevel;
            ProcessFile ??= source.ProcessFile;
            ProcessName ??= source.ProcessName;
            RestartAfterMillseconds ??= source.RestartAfterMillseconds;
            RetainPortsOnRestart ??= source.RetainPortsOnRestart;
            RuntimeDirectory ??= source.RuntimeDirectory;
            WaitForShutdownSeconds ??= source.WaitForShutdownSeconds;
            TrustAnchorsCertificate ??= source.TrustAnchorsCertificate;

            (Metrics ??= new DaprMetricsOptions()).EnrichFrom(source.Metrics);

            // Copy the environment variables - do it the old-fashioned way so we overwrite existing values
            // without getting key duplication errors.
            if (source.EnvironmentVariables?.Any() == true)
            {
                if (EnvironmentVariables == null)
                {
                    EnvironmentVariables = new Dictionary<string, string>();
                }

                foreach (var entry in source.EnvironmentVariables)
                {
                    EnvironmentVariables[entry.Key] = entry.Value;
                }
            }
        }

        /// <summary>
        /// Gets the address of the health endpoint, such as http://127.0.0.1:3500/v1.0/health.
        /// </summary>
        /// <returns>The health endpoint address.</returns>
        public Uri GetHealthUri() => GetLocalUri(builder => AddHealthUri(builder));

        /// <summary>
        /// Gets the address of the metrics endpoint, such as http://127.0.0.1:9090.
        /// </summary>
        /// <returns>The metrics endpoint address.</returns>
        public Uri GetMetricsUri() => GetLocalUri(builder => AddMetricsUri(builder));

        protected virtual bool AddHealthUri(UriBuilder builder) => false;

        protected virtual bool AddMetricsUri(UriBuilder builder) => false;

        protected Uri GetLocalUri(Func<UriBuilder, bool> configure)
        {
            var builder = new UriBuilder("http", DaprConstants.LocalhostAddress);
            if (configure(builder))
            {
                return builder.Uri;
            }
            else
            {
                return null;
            }
        }
    }
}
