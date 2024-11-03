namespace Man.Dapr.Sidekick
{
    public class DaprOptions : Options.DaprProcessOptions
    {
        public const string SectionName = "DaprSidekick";
        public const string EnvironmentVariablePrefix = "DAPRSIDEKICK_";

        public DaprOptions()
        {
            // Set defaults for all components
            LogLevel = Process.DaprProcessLogger.DebugLevel;
        }

        public DaprSidecarOptions Sidecar { get; set; } = new();

        public DaprPlacementOptions Placement { get; set; } = new();

        public DaprSchedulerOptions Scheduler { get; set; } = new();

        public DaprSentryOptions Sentry { get; set; } = new();

        /// <summary>
        /// Creates a deep clone of this instance.
        /// </summary>
        /// <returns>A deep clone of this insteance.</returns>
        public new DaprOptions Clone()
        {
            var clone = (DaprOptions)base.Clone();
            clone.Sidecar = Sidecar?.Clone();
            clone.Placement = Placement?.Clone();
            clone.Scheduler = Scheduler?.Clone();
            clone.Sentry = Sentry?.Clone();
            return clone;
        }
    }
}
