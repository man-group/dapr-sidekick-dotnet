using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Man.Dapr.Sidekick.AspNetCore.Metrics
{
    public class PrometheusLabelEnricher
    {
        private readonly IEnumerable<KeyValuePair<string, string>> _labels;

        public PrometheusLabelEnricher(IDictionary<string, string> labels)
        {
            // Construct the list of valid labels.
            _labels = labels?
                .Where(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Value))
                ?? Array.Empty<KeyValuePair<string, string>>();
        }

        public string EnrichMetricLine(string metricLine)
        {
            // If there are no labels then nothing to do
            if (!_labels.Any())
            {
                return metricLine;
            }

            // Find the end of the existing label block
            var labelEndIndex = metricLine.LastIndexOf('}');
            if (labelEndIndex > 0)
            {
                // Already has a label block
                return AddLabels(metricLine, labelEndIndex, true);
            }
            else
            {
                var spaceIndex = metricLine.IndexOf(' ');
                if (spaceIndex > 0)
                {
                    // Found the separator between metric name and value. Create the label block
                    return AddLabels(metricLine, spaceIndex, false);
                }
            }

            return metricLine;
        }

        // This method assumes line is a valid metric, and the labels are all valid key/value pairs.
        private string AddLabels(string line, int insertIndex, bool hasLabels)
        {
            var sb = new StringBuilder();

            // Add everything up to insert index
            sb.Append(line, 0, insertIndex);

            var appendComma = false;
            if (!hasLabels)
            {
                // Start the label block
                sb.Append('{');
            }
            else
            {
                // Label block already exists. If the last char is a comma we don't need to add one.
                appendComma = sb[sb.Length - 1] != ',';
            }

            foreach (var label in _labels)
            {
                var prefix = string.Concat(label.Key, "=\"");
                if (hasLabels && line.Contains(prefix))
                {
                    // Already exists. Do not replace.
                    continue;
                }

                // Append a comma if necessary
                if (appendComma)
                {
                    sb.Append(',');
                }
                else
                {
                    // We will need one next time
                    appendComma = true;
                }

                // Append the label
                sb.Append(prefix);
                sb.Append(label.Value);
                sb.Append('\"');
            }

            // End the label block if necessary
            if (!hasLabels)
            {
                sb.Append('}');
            }

            // Add the rest of the line
            sb.Append(line, insertIndex, line.Length - insertIndex);

            return sb.ToString();
        }
    }
}
