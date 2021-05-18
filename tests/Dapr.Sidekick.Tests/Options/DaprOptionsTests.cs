using NUnit.Framework;

namespace Dapr.Sidekick.Options
{
    public class DaprOptionsTests
    {
        public class Constructor
        {
            [Test]
            public void Should_set_defaults()
            {
                var options = new DaprOptions();
                Assert.That(options.LogLevel, Is.EqualTo(Process.DaprProcessLogger.DebugLevel));
            }
        }

        public class Clone
        {
            [Test]
            public void Should_not_throw_exception_when_uninitialized_properties()
            {
                Assert.DoesNotThrow(() => new DaprOptions().Clone());
            }

            [Test]
            public void Should_clone_all_members()
            {
                var source = new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions
                    {
                        AppId = "SIDECAR_APPID"
                    },
                    Placement = new DaprPlacementOptions
                    {
                        Id = "PLACEMENT_ID"
                    },
                    Sentry = new DaprSentryOptions
                    {
                        ProcessName = "SENTRY_PROCESSNAME"
                    }
                };

                var target = source.Clone();
                Compare(source, target);
            }
        }

        private static void Compare(DaprOptions source, DaprOptions target)
        {
            Assert.That(target, Is.Not.SameAs(source));
            Assert.That(target.Sidecar, Is.Not.SameAs(source.Sidecar));
            Assert.That(target.Sidecar.AppId, Is.EqualTo(source.Sidecar.AppId));
            Assert.That(target.Placement, Is.Not.SameAs(source.Placement));
            Assert.That(target.Placement.Id, Is.EqualTo(source.Placement.Id));
            Assert.That(target.Sentry, Is.Not.SameAs(source.Sentry));
            Assert.That(target.Sentry.ProcessName, Is.EqualTo(source.Sentry.ProcessName));
        }
    }
}
