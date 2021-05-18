using System.Collections.Generic;
using NUnit.Framework;

namespace Dapr.Sidekick.Options
{
    public class DaprMetricsOptionsTests
    {
        public class Clone
        {
            [Test]
            public void Should_not_throw_exception_when_uninitialized_properties()
            {
                Assert.DoesNotThrow(() => new DaprMetricsOptions().Clone());
            }

            [Test]
            public void Should_clone_all_members()
            {
                var source = InitValue();
                var target = source.Clone();
                Compare(source, target);
            }
        }

        public class EnrichFrom
        {
            [Test]
            public void Should_not_throw_exception_when_null_source()
            {
                Assert.DoesNotThrow(() => new DaprMetricsOptions().EnrichFrom(null));
            }

            [Test]
            public void Should_not_throw_exception_when_uninitialized_properties()
            {
                Assert.DoesNotThrow(() => new DaprMetricsOptions().EnrichFrom(new DaprMetricsOptions()));
            }

            [Test]
            public void Should_not_overwrite_specified_properties()
            {
                var source = InitValue();
                var target = InitValue();
                target.EnrichFrom(new DaprMetricsOptions());
                Compare(source, target, true); // Target should still entirely match original values
            }

            [Test]
            public void Should_enrich_all_unspecified_properties()
            {
                var source = InitValue();
                var target = new DaprMetricsOptions();
                target.EnrichFrom(source);
                Compare(source, target); // Target should entirely match new values in source
            }
        }

        public class SetLabel
        {
            [Test]
            public void Should_not_set_when_invalid_name()
            {
                var target = new DaprMetricsOptions();
                target.SetLabel(null, null);
                Assert.That(target.Labels, Is.Null);
            }

            [Test]
            public void Should_not_overwrite()
            {
                var target = new DaprMetricsOptions();
                target.SetLabel("TEST", "FIRST");
                Assert.That(target.Labels.Count, Is.EqualTo(1));
                Assert.That(target.Labels["TEST"], Is.EqualTo("FIRST"));

                target.SetLabel("TEST", "SECOND");
                Assert.That(target.Labels.Count, Is.EqualTo(1));
                Assert.That(target.Labels["TEST"], Is.EqualTo("FIRST"));
            }

            [Test]
            public void Should_overwrite()
            {
                var target = new DaprMetricsOptions();
                target.SetLabel("TEST", "FIRST");
                target.SetLabel("TEST", "SECOND", true);
                Assert.That(target.Labels.Count, Is.EqualTo(1));
                Assert.That(target.Labels["TEST"], Is.EqualTo("SECOND"));
            }
        }

        private static void Compare(DaprMetricsOptions source, DaprMetricsOptions target, bool sameInstances = false)
        {
            if (!sameInstances)
            {
                Assert.That(source, Is.Not.SameAs(target));
            }

            Assert.That(target.EnableCollector, Is.EqualTo(source.EnableCollector));
            Assert.That(target.Labels, Is.Not.Null);
            Assert.That(target.Labels, Is.EquivalentTo(source.Labels));
        }

        private static DaprMetricsOptions InitValue() => new DaprMetricsOptions
        {
            EnableCollector = true,
            Labels = new Dictionary<string, string>
            {
                { "1", "One" },
                { "2", "Two" }
            }
        };
    }
}
