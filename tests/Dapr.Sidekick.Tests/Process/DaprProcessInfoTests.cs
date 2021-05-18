using System;
using NUnit.Framework;

namespace Dapr.Sidekick.Process
{
    public class DaprProcessInfoTests
    {
        public class Unknown
        {
            [Test]
            public void Should_return_new_instance_each_call()
            {
                Assert.That(DaprProcessInfo.Unknown, Is.Not.SameAs(DaprProcessInfo.Unknown));
            }

            [Test]
            public void Should_set_expected_properties()
            {
                var pi = DaprProcessInfo.Unknown;
                Assert.That(pi.Name, Is.EqualTo("unknown"));
                Assert.That(pi.Id, Is.Null);
                Assert.That(pi.Version, Is.Null);
                Assert.That(pi.Status, Is.EqualTo(DaprProcessStatus.Stopped));
            }
        }

        public class Constructor
        {
            [Test]
            public void Should_set_properties()
            {
                var pi = new DaprProcessInfo("TEST", 100, "VERSION", DaprProcessStatus.Started);
                Assert.That(pi.Name, Is.EqualTo("TEST"));
                Assert.That(pi.Id, Is.EqualTo(100));
                Assert.That(pi.Version, Is.EqualTo("VERSION"));
                Assert.That(pi.Status, Is.EqualTo(DaprProcessStatus.Started));
            }
        }

        public class IsRunning
        {
            [Test]
            public void Should_return_true_for_started_only()
            {
                foreach (DaprProcessStatus processStatus in Enum.GetValues(typeof(DaprProcessStatus)))
                {
                    Assert.That(new DaprProcessInfo("TEST", 100, null, processStatus).IsRunning, Is.EqualTo(processStatus == DaprProcessStatus.Started));
                }
            }
        }

        public class Description
        {
            [Test]
            public void Should_return_running()
            {
                Assert.That(new DaprProcessInfo("TEST", 100, null, DaprProcessStatus.Started).Description, Is.EqualTo("Dapr process 'TEST' running, unverified version"));
            }

            [Test]
            public void Should_return_running_with_version()
            {
                Assert.That(new DaprProcessInfo("TEST", 100, "VERSION", DaprProcessStatus.Started).Description, Is.EqualTo("Dapr process 'TEST' running, version VERSION"));
            }

            [Test]
            public void Should_return_not_available()
            {
                Assert.That(new DaprProcessInfo("TEST", 100, null, DaprProcessStatus.Stopped).Description, Is.EqualTo("Dapr process 'TEST' not available, status is Stopped"));
            }
        }

        public class ToStringMethod
        {
            [Test]
            public void Should_return_description()
            {
                var pi = new DaprProcessInfo("TEST", 100, "VERSION", DaprProcessStatus.Started);
                Assert.That(pi.ToString(), Is.EqualTo(pi.Description));
            }
        }
    }
}
