using System.Collections.Generic;

namespace Dapr.Sidekick
{
    public class DaprMetricsOptions
    {
        /// <summary>
        /// Gets or sets a value that determines whether the metrics collector is enabled for this component (default true).
        /// </summary>
        public bool? EnableCollector { get; set; }

        /// <summary>
        /// Gets or sets the labels to add to each metric line.
        /// </summary>
        public IDictionary<string, string> Labels { get; set; }

        /// <summary>
        /// Creates a deep clone of this instance.
        /// </summary>
        /// <returns>A deep clone of this insteance.</returns>
        public DaprMetricsOptions Clone() => (DaprMetricsOptions)MemberwiseClone();

        /// <summary>
        /// Updates any undefined properties in this instance with the values in <paramref name="source"/> where specified.
        /// </summary>
        /// <param name="source">A source options instance.</param>
        public void EnrichFrom(DaprMetricsOptions source)
        {
            if (source == null)
            {
                return;
            }

            EnableCollector ??= source.EnableCollector;

            if (source.Labels != null)
            {
                foreach (var label in source.Labels)
                {
                    SetLabel(label.Key, label.Value);
                }
            }
        }

        public void SetLabel(string name, string value, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (Labels == null)
            {
                Labels = new Dictionary<string, string>();
            }

            // Set the label if not already existing or the overwrite flag is specified
            if (!Labels.ContainsKey(name) || overwrite)
            {
                Labels[name] = value;
            }
        }
    }
}
