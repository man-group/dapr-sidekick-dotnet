using System.Linq;
using Man.Dapr.Sidekick.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class DaprProcessTests
    {
        public class Start
        {
            [Test]
            public void Should_not_start_when_not_enabled()
            {
                var logger = Substitute.For<IDaprLogger>();
                var filename = TestResourceHelper.CompileTestSystemProcessExe();
                var p = new MockDaprProcess()
                {
                    ProcessFinder = Substitute.For<IProcessFinder>()
                };

                var options = new DaprOptions()
                {
                    Enabled = false
                };

                try
                {
                    // Start (asynchronous) and wait for started
                    p.Start(() => options, logger);
                    var loopCount = 0;
                    do
                    {
                        loopCount++;
                        var pi = p.GetProcessInfo();
                        if (pi.Status == DaprProcessStatus.Disabled)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(10);
                    }
                    while (loopCount < 100); // 1 second

                    // Assert running
                    Assert.That(p.GetProcessInfo().Status, Is.EqualTo(DaprProcessStatus.Disabled));
                    var loggerCalls = logger.ReceivedLoggerCalls();
                }
                finally
                {
                    try
                    {
                        // Assert disabled
                        Assert.That(p.GetProcessInfo().Status, Is.EqualTo(DaprProcessStatus.Disabled));

                        // Stop the process and wait for exit
                        var loopCount = 0;
                        do
                        {
                            p.Stop();
                            var pi = p.GetProcessInfo();
                            if (pi.Id is null)
                            {
                                break;
                            }

                            loopCount++;
                            System.Threading.Thread.Sleep(100);
                        }
                        while (loopCount < 10); // 1 second

                        // Assert still Disabled (i.e. didn't enter the Stopping/Stopped states
                        Assert.That(p.GetProcessInfo().Status, Is.EqualTo(DaprProcessStatus.Disabled));
                    }
                    finally
                    {
                        // Delete the resource file
                        TestResourceHelper.DeleteTestProcess(filename);
                    }
                }
            }

            [Test]
            public void Should_start_and_stop_managed_process()
            {
                var logger = Substitute.For<IDaprLogger>();
                var filename = TestResourceHelper.CompileTestSystemProcessExe();
                var p = new MockDaprProcess()
                {
                    ProcessFinder = Substitute.For<IProcessFinder>()
                };
                var options = new DaprOptions()
                {
                    ProcessFile = filename
                };

                try
                {
                    // Start (asynchronous) and wait for started
                    p.Start(() => options, logger);
                    var loopCount = 0;
                    do
                    {
                        loopCount++;
                        var pi = p.GetProcessInfo();
                        if (pi.Status == DaprProcessStatus.Starting && pi.Id > 0)
                        {
                            // Process is created, set status to started to simulate completed startup
                            p.UpdateStatus(DaprProcessStatus.Started);
                            break;
                        }

                        System.Threading.Thread.Sleep(10);
                    }
                    while (loopCount < 100); // 1 second

                    // Assert running
                    var processInfo = p.GetProcessInfo();
                    Assert.That(processInfo.IsRunning, Is.True);
                    Assert.That(processInfo.IsAttached, Is.False);
                }
                finally
                {
                    try
                    {
                        // Stop the process and wait for exit
                        var loopCount = 0;
                        do
                        {
                            var pi = p.GetProcessInfo();
                            if (pi.Id is null)
                            {
                                break;
                            }

                            loopCount++;
                            p.Stop();
                            System.Threading.Thread.Sleep(100);
                        }
                        while (loopCount < 10); // 1 second

                        // Assert stopped
                        Assert.That(p.GetProcessInfo().Status, Is.EqualTo(DaprProcessStatus.Stopped));
                    }
                    finally
                    {
                        // Delete the resource file
                        TestResourceHelper.DeleteTestProcess(filename);
                    }
                }
            }

            [Test]
            public void Should_attach_existing_process()
            {
                var logger = Substitute.For<IDaprLogger>();
                var p = new MockDaprProcess(ProcessComparison.Attachable)
                {
                    ProcessFinder = Substitute.For<IProcessFinder>()
                };
                var options = new DaprOptions();
                var existingProcess = Substitute.For<IProcess>();
                existingProcess.Id.Returns(1234);
                existingProcess.Name.Returns(MockDaprProcess.DefaultName);
                p.ProcessFinder.FindExistingProcesses(MockDaprProcess.DefaultName).Returns(new[] { existingProcess });
                p.Arguments.Add("ARG1", "VAL1");
                existingProcess.GetCommandLine().GetArgumentsAsDictionary('-').Returns(p.Arguments);

                try
                {
                    // Start (asynchronous) and wait for started
                    p.Start(() => options, logger);
                    var loopCount = 0;
                    do
                    {
                        loopCount++;
                        var pi = p.GetProcessInfo();
                        if (pi.Status == DaprProcessStatus.Started)
                        {
                            // Process is attached
                            break;
                        }

                        System.Threading.Thread.Sleep(10);
                    }
                    while (loopCount < 100); // 1 second

                    // Assert attached and we have a logger call
                    var processInfo = p.GetProcessInfo();
                    Assert.That(processInfo.IsRunning, Is.True);
                    Assert.That(processInfo.IsAttached, Is.True);
                    var loggerCalls = logger.ReceivedLoggerCalls();
                    Assert.That(
                        System.Array.Find(loggerCalls, x => x.Message == "Attached to existing Dapr Process Mock PID:1234"),
                        Is.Not.Null);
                }
                finally
                {
                    // Stop the process and ensure attached process did not receive Stop.
                    p.Stop();
                    existingProcess.DidNotReceive().Stop(Arg.Any<int?>());
                }
            }

            [Test]
            public void Should_throw_exception_when_duplicate_process()
            {
                var logger = Substitute.For<IDaprLogger>();
                var p = new MockDaprProcess(ProcessComparison.Duplicate)
                {
                    ProcessFinder = Substitute.For<IProcessFinder>()
                };
                var options = new DaprOptions();
                var existingProcess = Substitute.For<IProcess>();
                existingProcess.Id.Returns(1234);
                p.ProcessFinder.FindExistingProcesses(MockDaprProcess.DefaultName).Returns(new[] { existingProcess });

                try
                {
                    // Start (asynchronous) and wait for initialized and stopped
                    p.Start(() => options, logger);
                    var loopCount = 0;
                    var hasInitialized = false;
                    do
                    {
                        loopCount++;
                        var pi = p.GetProcessInfo();
                        if (p.ProcessFinder.ReceivedCalls().Any() && !hasInitialized)
                        {
                            // Check completed. Wait for stopped
                            hasInitialized = true;
                        }
                        else if (pi.Status == DaprProcessStatus.Stopped && hasInitialized)
                        {
                            // Stopped
                            break;
                        }

                        System.Threading.Thread.Sleep(10);
                    }
                    while (loopCount < 100); // 1 second

                    // Make sure it was initialized and we have a logger call
                    Assert.That(hasInitialized, Is.True);
                    var loggerCalls = logger.ReceivedLoggerCalls();
                    Assert.That(loggerCalls.Length, Is.GreaterThan(1));
                    var lastCall = loggerCalls[loggerCalls.Length - 1];
                    Assert.That(lastCall.Message, Is.EqualTo("Process Mock failed to start, a restart will not be attempted"));
                    Assert.That(lastCall.LogLevel, Is.EqualTo(DaprLogLevel.Error));
                    Assert.That(lastCall.Exception, Is.Not.Null);
                    Assert.That(lastCall.Exception.Message, Is.EqualTo("Process  PID:1234 is already running with duplicate settings. Dapr does not permit duplicate equivalent instances on a single host."));
                }
                finally
                {
                    // Stop the process and wait for exit
                    var loopCount = 0;
                    do
                    {
                        loopCount++;
                        p.Stop();
                        var pi = p.GetProcessInfo();
                        if (pi.Status == DaprProcessStatus.Stopped)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(10);
                    }
                    while (loopCount < 100); // 1 second
                }
            }
        }
    }
}
