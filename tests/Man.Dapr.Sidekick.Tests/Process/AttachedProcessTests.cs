using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class AttachedProcessTests
    {
        public class Id
        {
            [Test]
            public void Should_return_process_value()
            {
                var p = Substitute.For<IProcess>();
                p.Id.Returns(100);
                Assert.That(new AttachedProcess(p).Id, Is.EqualTo(100));
            }

            [Test]
            public void Should_return_null_when_process_null()
            {
                Assert.That(new AttachedProcess(null).Id, Is.Null);
            }

            public class Name
            {
                [Test]
                public void Should_return_process_value()
                {
                    var p = Substitute.For<IProcess>();
                    p.Name.Returns("TEST");
                    Assert.That(new AttachedProcess(p).Name, Is.EqualTo("TEST"));
                }

                [Test]
                public void Should_return_null_when_process_null()
                {
                    Assert.That(new AttachedProcess(null).Name, Is.Null);
                }
            }

            public class IsRunning
            {
                [Test]
                public void Should_return_process_value()
                {
                    var p = Substitute.For<IProcess>();
                    p.IsRunning.Returns(true);
                    Assert.That(new AttachedProcess(p).IsRunning, Is.True);
                }

                [Test]
                public void Should_return_false_when_process_null()
                {
                    Assert.That(new AttachedProcess(null).IsRunning, Is.False);
                }
            }

            public class GetCommandLine
            {
                [Test]
                public void Should_return_process_value()
                {
                    var p = Substitute.For<IProcess>();
                    var cmd = Substitute.For<IProcessCommandLine>();
                    p.GetCommandLine().Returns(cmd);
                    Assert.That(new AttachedProcess(p).GetCommandLine(), Is.SameAs(cmd));
                }

                [Test]
                public void Should_return_null_when_process_null()
                {
                    Assert.That(new AttachedProcess(null).GetCommandLine(), Is.Null);
                }
            }

            public class Stop
            {
                [Test]
                public void Should_set_process_null()
                {
                    var p = Substitute.For<IProcess>();
                    var ap = new AttachedProcess(p);
                    Assert.That(ap.GetCommandLine(), Is.Not.Null);

                    ap.Stop();
                    Assert.That(ap.GetCommandLine(), Is.Null);
                }
            }
        }
    }
}
