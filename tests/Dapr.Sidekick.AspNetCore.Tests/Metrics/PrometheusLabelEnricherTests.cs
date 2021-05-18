using System.Collections.Generic;
using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public class PrometheusLabelEnricherTests
    {
        [Test]
        public void Should_not_enrich_when_no_labels()
        {
            var enricher = new PrometheusLabelEnricher(new Dictionary<string, string>());

            Assert.That(enricher.EnrichMetricLine("metric_one"), Is.EqualTo("metric_one"));
        }

        [Test]
        public void Should_not_enrich_when_no_closing_block()
        {
            var enricher = new PrometheusLabelEnricher(new Dictionary<string, string>
            {
                { "label_one", "value_one" }
            });

            Assert.That(enricher.EnrichMetricLine("metric_one"), Is.EqualTo("metric_one"));
        }

        [Test]
        public void Should_not_enrich_duplicate_label()
        {
            var enricher = new PrometheusLabelEnricher(new Dictionary<string, string>
            {
                { "label_one", "value_one" }
            });

            Assert.That(enricher.EnrichMetricLine("metric_one{label_one=\"value_two\"} value_one"), Is.EqualTo("metric_one{label_one=\"value_two\"} value_one"));
        }

        [Test]
        public void Should_enrich_new_label_section()
        {
            var enricher = new PrometheusLabelEnricher(new Dictionary<string, string>
            {
                { "label_one", "value_one" }
            });

            Assert.That(enricher.EnrichMetricLine("metric_one value_one"), Is.EqualTo("metric_one{label_one=\"value_one\"} value_one"));
        }

        [Test]
        public void Should_enrich_existing_label_section()
        {
            var enricher = new PrometheusLabelEnricher(new Dictionary<string, string>
            {
                { "label_two", "value_two" }
            });

            Assert.That(enricher.EnrichMetricLine("metric_one{label_one=\"value_one\"} value_one"), Is.EqualTo("metric_one{label_one=\"value_one\",label_two=\"value_two\"} value_one"));
        }

        [Test]
        public void Should_enrich_existing_multiple_labels()
        {
            var enricher = new PrometheusLabelEnricher(new Dictionary<string, string>
            {
                { "label_two", "value_two" },
                { "label_three", "value_three" }
            });

            Assert.That(enricher.EnrichMetricLine("metric_one{label_one=\"value_one\"} value_one"), Is.EqualTo("metric_one{label_one=\"value_one\",label_two=\"value_two\",label_three=\"value_three\"} value_one"));
        }

        [Test]
        public void Should_enrich_line_with_tokens()
        {
            var enricher = new PrometheusLabelEnricher(new Dictionary<string, string>
            {
                { "label_two", "value_two" }
            });

            Assert.That(enricher.EnrichMetricLine("metric_one{label_one=\"http://some_value/{some_id}/{some_path}/something\"} value_one"), Is.EqualTo("metric_one{label_one=\"http://some_value/{some_id}/{some_path}/something\",label_two=\"value_two\"} value_one"));
        }
    }
}
