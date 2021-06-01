using System;
using Man.Dapr.Sidekick.Http;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Threading;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class DaprProcessHostTests
    {
        [Test]
        public void Should_start_and_stop()
        {
            var process = Substitute.For<IDaprProcess<MockDaprProcessOptions>>();
            var processInfo = new DaprProcessInfo("NAME", 1234, "VERSION", DaprProcessStatus.Started, false);
            var processHost = new MockDaprProcessHost(process);
            var options = new DaprOptions();
            var mockOptions = new MockDaprProcessOptions();
            var cancellationToken = DaprCancellationToken.None;
            var startingArgs = new DaprProcessStartingEventArgs<MockDaprProcessOptions>(mockOptions);
            var stoppingArgs = new DaprProcessStoppingEventArgs(cancellationToken);
            Func<DaprOptions> optionsAccessor = () => options;

            // Start
            process.LastSuccessfulOptions.Returns(mockOptions);
            process.GetProcessInfo().Returns(processInfo);
            process.Start(optionsAccessor, processHost.Logger, cancellationToken).Returns(true);
            Assert.That(processHost.Start(optionsAccessor, cancellationToken), Is.True);
            Assert.That(processHost.Logger, Is.Not.Null);
            Assert.That(processHost.DaprHttpClientFactory, Is.Not.Null);
            Assert.That(processHost.GetProcessInfo(), Is.SameAs(processInfo));
            Assert.That(processHost.GetProcessOptions(), Is.SameAs(mockOptions));
            Assert.That(((IDaprProcessHost)processHost).GetProcessOptions(), Is.SameAs(mockOptions));

            // Fire starting event, make sure override is called.
            processHost.Process.Starting += Raise.EventWith(this, startingArgs);
            Assert.That(processHost.StartingArgs, Is.SameAs(startingArgs));

            // Fire stopping event, make sure override is called.
            processHost.Process.Stopping += Raise.EventWith(this, stoppingArgs);
            Assert.That(processHost.StoppingArgs, Is.SameAs(stoppingArgs));

            // Stop
            processHost.Stop(cancellationToken);
            Assert.That(processHost.GetProcessInfo().Status, Is.EqualTo(DaprProcessStatus.Stopped));
            Assert.That(processHost.GetProcessOptions(), Is.Null);
        }

        public class MockDaprProcessHost : DaprProcessHost<MockDaprProcessOptions>
        {
            public MockDaprProcessHost(IDaprProcess<MockDaprProcessOptions> process)
                : base(
                      () => process,
                      Substitute.For<IDaprProcessHttpClientFactory>(),
                      Substitute.For<IDaprLogger>())
            {
            }

            public DaprProcessStartingEventArgs<MockDaprProcessOptions> StartingArgs { get; private set; }

            public DaprProcessStoppingEventArgs StoppingArgs { get; private set; }

            protected override void OnProcessStarting(DaprProcessStartingEventArgs<MockDaprProcessOptions> args)
            {
                StartingArgs = args;
                base.OnProcessStarting(args);
            }

            protected override void OnProcessStopping(DaprProcessStoppingEventArgs args)
            {
                StoppingArgs = args;
                base.OnProcessStopping(args);
            }
        }
    }
}
