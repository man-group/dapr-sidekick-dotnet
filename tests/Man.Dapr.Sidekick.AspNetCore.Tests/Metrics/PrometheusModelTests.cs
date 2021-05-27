using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore.Metrics
{
    public class PrometheusModelTests
    {
        public class Constructor
        {
            [Test]
            public void Should_initialize_properties()
            {
                var model = new PrometheusModel();
                Assert.That(model.Metrics, Is.Not.Null);
                Assert.That(model.Unknown, Is.Not.Null);
            }
        }

        public class GetOrAddMetric
        {
            [Test]
            public void Should_add_new()
            {
                var model = new PrometheusModel();
                var metric = model.GetOrAddMetric("TEST");
                Assert.That(metric, Is.Not.Null);
                Assert.That(metric.Name, Is.EqualTo("TEST"));
                Assert.That(metric.ToString(), Is.EqualTo("TEST"));
            }

            [Test]
            public void Should_get_existing()
            {
                var model = new PrometheusModel();
                var metric1 = model.GetOrAddMetric("TEST");
                var metric2 = model.GetOrAddMetric("TEST");
                Assert.That(metric1, Is.SameAs(metric2));
            }
        }
    }
}
