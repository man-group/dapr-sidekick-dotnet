using NUnit.Framework;

namespace Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarMetricFilterTests
    {
        public class ExcludeMetricLine
        {
            [TestCase("EMPTY", false)]
            [TestCase("status=401", false)]
            [TestCase("STATUS=\"401\"", false)]
            [TestCase(" status=\"401\" ", true)]
            public void Should_exclude_expected_lines(string line, bool expected)
            {
                var filter = new DaprSidecarMetricFilter();
                Assert.That(filter.ExcludeMetricLine(null, line), Is.EqualTo(expected));
            }
        }
    }
}
