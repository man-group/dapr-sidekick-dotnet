namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public interface IPrometheusMetricFilter
    {
        /// <summary>
        /// Get a value that determines if the metric line should be excluded.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="lineText">The full text of the metric line in Prometheus format.</param>
        /// <returns><c>true</c> if the metric line should be ignored, else <c>false</c>.</returns>
        bool ExcludeMetricLine(string name, string lineText);
    }
}
