using System;
using Man.Dapr.Sidekick.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class SystemProcessTests
    {
        public class Constructor
        {
            [Test]
            public void Should_store_process()
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var systemProcess = new SystemProcess(process);

                Assert.That(systemProcess.Process, Is.SameAs(process));
                Assert.That(systemProcess.Id, Is.EqualTo(process.Id));
                Assert.That(systemProcess.Name, Is.EqualTo(process.ProcessName));
                Assert.That(systemProcess.IsRunning, Is.True);
            }
        }

        public class GetCommandLine
        {
            [Test]
            public void Should_get_for_process()
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var systemProcess = new SystemProcess(process);
                var commandLine = systemProcess.GetCommandLine();

                Assert.That(commandLine, Is.Not.Null);
                Assert.That(commandLine.Process, Is.SameAs(systemProcess));
            }
        }

        public class Start
        {
            [Test]
            public void Should_invoke_controller()
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var controller = Substitute.For<ISystemProcessController>();
                var systemProcess = new SystemProcess(process, controller: controller);

                systemProcess.Start();
                controller.Received(1).Start();
            }
        }

        public class Stop
        {
            [Test]
            public void Should_kill()
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var systemProcess = new SystemProcess(process, logger, controller);

                systemProcess.Stop(null);
                controller.Received(1).Kill();

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(1));
                Assert.That(loggerCalls[0].Message, Is.EqualTo($"Killing Process {systemProcess.Name} PID:{systemProcess.Id}..."));
                Assert.That(loggerCalls[0].LogLevel, Is.EqualTo(DaprLogLevel.Information));
            }

            [Test]
            public void Should_catch_exception_from_kill()
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var systemProcess = new SystemProcess(process, logger, controller);

                var ex = new Exception();
                controller.When(x => x.Kill()).Do(_ => throw ex);
                systemProcess.Stop(null);

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(2));
                Assert.That(loggerCalls[1].Message, Is.EqualTo("Error killing Process"));
                Assert.That(loggerCalls[1].LogLevel, Is.EqualTo(DaprLogLevel.Error));
                Assert.That(loggerCalls[1].Exception, Is.SameAs(ex));
            }

#if !NET35
            [Test]
            public void Should_wait_and_exit_on_cancellation()
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var systemProcess = new SystemProcess(process, logger, controller);
                var cts = new System.Threading.CancellationTokenSource();
                var token = new Man.Dapr.Sidekick.Threading.DaprCancellationToken(cts.Token);

                // Wait 1 second, cancel after 1 second
                var start = DateTime.Now;
                var timer = new System.Timers.Timer
                {
                    AutoReset = false,
                    Interval = 100 // Should allow at least one loop
                };
                timer.Elapsed += (sender, e) => cts.Cancel();
                timer.Start();

                // Signal stop, waiting for several seconds (should exit after 500ms due to cancellation)
                systemProcess.Stop(6, token);
                Assert.That(DateTime.Now.Subtract(start).TotalSeconds < 5);

                // Should not be a graceful shutdown
                controller.Received().WaitForExit(100);
                controller.Received(1).Kill();

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(2));
                Assert.That(loggerCalls[0].Message, Is.EqualTo($"Waiting 6 second(s) for {systemProcess.Name} to stop..."));
                Assert.That(loggerCalls[0].LogLevel, Is.EqualTo(DaprLogLevel.Information));
            }
#endif

            [Test]
            public void Should_catch_exception_from_waitforexit()
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var systemProcess = new SystemProcess(process, logger, controller);

                var ex = new Exception();
                controller.When(x => x.WaitForExit(100)).Do(_ => throw ex);
                systemProcess.Stop(1);

                // Should not be a graceful shutdown
                controller.Received(1).Kill();

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(3));
                Assert.That(loggerCalls[1].Message, Is.EqualTo("Error stopping Process"));
                Assert.That(loggerCalls[1].LogLevel, Is.EqualTo(DaprLogLevel.Error));
                Assert.That(loggerCalls[1].Exception, Is.SameAs(ex));
            }

            [Test]
            public void Should_stop_gracefully()
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var systemProcess = new SystemProcess(process, logger, controller);

                controller.WaitForExit(Arg.Any<int>()).Returns(true);
                systemProcess.Stop(1);

                // Should be a graceful shutdown
                controller.DidNotReceive().Kill();
                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(1));
            }

            [Test]
            public void Should_wait_for_exit_when_not_running()
            {
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var systemProcess = new SystemProcess(null, logger, controller);

                systemProcess.Stop(null);
                controller.Received(1).WaitForExit();
                controller.DidNotReceive().Kill();
            }

            [Test]
            public void Should_catch_exception_from_wait_for_exit_when_not_running()
            {
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var systemProcess = new SystemProcess(null, logger, controller);

                var ex = new Exception();
                controller.When(x => x.WaitForExit()).Do(_ => throw ex);
                systemProcess.Stop(null);

                // Should not be a graceful shutdown
                controller.Received(1).Kill();

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(2));
                Assert.That(loggerCalls[0].Message, Is.EqualTo("Error waiting for Process to exit"));
                Assert.That(loggerCalls[0].LogLevel, Is.EqualTo(DaprLogLevel.Error));
                Assert.That(loggerCalls[0].Exception, Is.SameAs(ex));
            }
        }
    }
}
