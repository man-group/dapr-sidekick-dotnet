using System.Collections.Generic;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public class PrometheusModel
    {
        public class Metric
        {
            public Metric(string name)
            {
                Name = name;
                MetricLines = new List<string>();
            }

            public string Name { get; }

            public string HelpLine { get; set; }

            public string TypeLine { get; set; }

            public List<string> MetricLines { get; }

            public override string ToString() => Name;
        }

        public PrometheusModel()
        {
            Metrics = new Dictionary<string, Metric>();
            Unknown = new List<string>();
        }

        public Dictionary<string, Metric> Metrics { get; }

        public List<string> Unknown { get; }

        internal Metric GetOrAddMetric(string name)
        {
            if (Metrics.ContainsKey(name))
            {
                return Metrics[name];
            }
            else
            {
                var metric = new Metric(name);
                Metrics.Add(name, metric);
                return metric;
            }
        }
    }
}
