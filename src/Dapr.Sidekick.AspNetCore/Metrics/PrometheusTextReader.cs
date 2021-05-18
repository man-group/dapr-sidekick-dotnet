using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public class PrometheusTextReader
    {
        private const string HelpPrefix = "# HELP";
        private const string TypePrefix = "# TYPE";

        private enum LineType
        {
            Metric = 0,
            Help = 1,
            Type = 2,
            Empty = 3,
            Unknown = 4
        }

        private readonly PrometheusModel _model;
        private readonly PrometheusLabelEnricher _labelEnricher = null;

        public PrometheusTextReader(PrometheusModel model, PrometheusLabelEnricher labelEnricher = null)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _labelEnricher = labelEnricher;
        }

        public async Task ReadAsync(Stream source, CancellationToken cancellationToken)
        {
            // Create a reader for the stream
            using var reader = new StreamReader(source);

            // Read each line
            string sourceLine;
            PrometheusModel.Metric currentMetric = null;
            while ((sourceLine = await reader.ReadLineAsync()) != null)
            {
                // Check the cancellation token
                cancellationToken.ThrowIfCancellationRequested();

                // Identify it
                var (lineType, name) = IdentifyLine(sourceLine);
                if (string.IsNullOrEmpty(name) || lineType == LineType.Unknown || lineType == LineType.Empty)
                {
                    // Unknown line
                    _model.Unknown.Add(sourceLine);
                    continue;
                }

                // If the current metric has the same name it can be reused for efficiency, else we'll need to add/retrieve another one.
                if (currentMetric?.Name != name)
                {
                    // Different name. Get the metric
                    currentMetric = _model.GetOrAddMetric(name);
                }

                // Set the value if not already specified
                switch (lineType)
                {
                    case LineType.Help:
                        currentMetric.HelpLine ??= sourceLine;
                        break;

                    case LineType.Type:
                        currentMetric.TypeLine ??= sourceLine;
                        break;

                    case LineType.Metric:
                        currentMetric.MetricLines.Add(_labelEnricher?.EnrichMetricLine(sourceLine) ?? sourceLine);
                        break;
                }
            }
        }

        private (LineType lineType, string name) IdentifyLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return (LineType.Empty, null);
            }

            if (line.StartsWith(HelpPrefix))
            {
                // HELP definition
                return (LineType.Help, GetName(line, HelpPrefix.Length, ' '));
            }
            else if (line.StartsWith(TypePrefix))
            {
                // TYPE definition
                return (LineType.Type, GetName(line, TypePrefix.Length, ' '));
            }
            else if (line[0] != '#')
            {
                // Metric definition
                return (LineType.Metric, GetName(line, 0, ' ', '{'));
            }
            else
            {
                // Unknown
                return (LineType.Unknown, null);
            }
        }

        private string GetName(string line, int startIndex, params char[] terminators)
        {
            // Simple way of skipping over any leading spaces, faster than Trim
            var firstNonSpace = startIndex;
            while (line[firstNonSpace] == ' ')
            {
                firstNonSpace++;
            }

            // Check each character to see if it is a terminator, when it is return the substring up to that point.
            for (var i = firstNonSpace; i < line.Length; i++)
            {
                if (terminators.Any(terminator => terminator == line[i]))
                {
#if NETCOREAPP
                    return line[firstNonSpace..i];
#else
                    return line.Substring(firstNonSpace, i - firstNonSpace);
#endif
                }
            }

            // Not found
            return null;
        }
    }
}
