using System;
using System.Collections.Specialized;

namespace Man.Dapr.Sidekick.Options
{
    public class DaprManagedProcessOptions
    {
        /// <summary>
        /// Gets or sets a working directory to be used by the managed process.
        /// This is required by some components that use relevant directory paths to load local files from the project folders.
        /// Defaults to %USERPROFILE%/.dapr ($HOME/.dapr on Linux) if not specified.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets optional command-line process arguments.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Gets or sets an optional action for configuring any environment variables for the process.
        /// </summary>
        public Action<StringDictionary> ConfigureEnvironmentVariables { get; set; }
    }
}
