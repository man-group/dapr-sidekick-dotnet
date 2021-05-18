using NUnit.Framework;

namespace Dapr.Sidekick.Process
{
    public class DaprProcessFactoryTests
    {
        public class CreateDaprSidecarProcess
        {
            [Test]
            public void Should_create_new_instance_Each_call()
            {
                var factory = new DaprProcessFactory();
                var p1 = factory.CreateDaprSidecarProcess();
                var p2 = factory.CreateDaprSidecarProcess();

                Assert.That(p1, Is.Not.Null);
                Assert.That(p2, Is.Not.Null);
                Assert.That(p2, Is.Not.SameAs(p1));
            }
        }

        public class CreateDaprPlacementProcess
        {
            [Test]
            public void Should_create_new_instance_Each_call()
            {
                var factory = new DaprProcessFactory();
                var p1 = factory.CreateDaprPlacementProcess();
                var p2 = factory.CreateDaprPlacementProcess();

                Assert.That(p1, Is.Not.Null);
                Assert.That(p2, Is.Not.Null);
                Assert.That(p2, Is.Not.SameAs(p1));
            }
        }

        public class CreateDaprSentryProcess
        {
            [Test]
            public void Should_create_new_instance_Each_call()
            {
                var factory = new DaprProcessFactory();
                var p1 = factory.CreateDaprSentryProcess();
                var p2 = factory.CreateDaprSentryProcess();

                Assert.That(p1, Is.Not.Null);
                Assert.That(p2, Is.Not.Null);
                Assert.That(p2, Is.Not.SameAs(p1));
            }
        }
    }
}
