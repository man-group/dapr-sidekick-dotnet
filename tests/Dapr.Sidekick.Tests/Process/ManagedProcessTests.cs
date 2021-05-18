using System;
using System.Collections.Specialized;
#if NETFRAMEWORK
using System.Linq;
#endif
using Dapr.Sidekick.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.Process
{
    public class ManagedProcessTests
    {
        private static readonly string ProcessFilename = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        public class Constructor
        {
            [Test]
            public void Should_initialize_properties()
            {
                var p = new ManagedProcess();
                Assert.That(p.Id, Is.Null);
                Assert.That(p.Name, Is.Null);
                Assert.That(p.IsRunning, Is.False);
                Assert.That(p.CreateSystemProcess, Is.Not.Null);
            }
        }

        public class Start
        {
            [TestCase(null)]
            [TestCase("")]
            [TestCase("DOES_NOT_EXIST")]
            public void Should_throw_exception_when_invalid_filename(string filename)
            {
                var controller = Substitute.For<ISystemProcessController>();
                var mp = new ManagedProcess { CreateSystemProcess = p => new SystemProcess(p, controller: controller) };

                Assert.Throws(
                    Is.InstanceOf<InvalidOperationException>().With.Message.EqualTo($"Unable to start process, file '{filename}' does not exist"),
                    () => mp.Start(filename));
            }

#if !NET35
            [Test]
            public void Should_not_start_when_cancellation_requested()
            {
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var mp = new ManagedProcess { CreateSystemProcess = p => new SystemProcess(p, logger, controller) };
                var cts = new System.Threading.CancellationTokenSource();

                cts.Cancel();
                mp.Start(ProcessFilename, cancellationToken: new Threading.DaprCancellationToken(cts.Token));
            }

#endif

            [Test]
            public void Should_start_process()
            {
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                SystemProcess sp = null;
                System.Diagnostics.Process p = null;
                var mp = new ManagedProcess
                {
                    CreateSystemProcess = process =>
                    {
                        p = process;
                        sp = new SystemProcess(null, logger, controller);
                        return sp;
                    }
                };

                mp.Start(ProcessFilename, "ARG1=VAL1", logger);
                Assert.That(p, Is.Not.Null);

                var psi = p.StartInfo;
                Assert.That(psi.FileName, Is.EqualTo(ProcessFilename));
                Assert.That(psi.Arguments, Is.EqualTo("ARG1=VAL1"));
                Assert.That(psi.UseShellExecute, Is.False);
                Assert.That(psi.CreateNoWindow, Is.True);
                Assert.That(psi.RedirectStandardOutput, Is.True);
                Assert.That(psi.RedirectStandardError, Is.True);
                Assert.That(psi.RedirectStandardInput, Is.True);
                Assert.That(psi.WorkingDirectory, Is.Not.Empty);
                Assert.That(p.EnableRaisingEvents, Is.True);

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(2));
                Assert.That(loggerCalls[0].Message, Is.EqualTo($"Starting Process {ProcessFilename} with arguments 'ARG1=VAL1'"));
                Assert.That(loggerCalls[0].LogLevel, Is.EqualTo(DaprLogLevel.Information));
                Assert.That(loggerCalls.Length, Is.EqualTo(2));
                Assert.That(loggerCalls[1].Message, Is.EqualTo("Process (null) PID:(null) started successfully"));
                Assert.That(loggerCalls[1].LogLevel, Is.EqualTo(DaprLogLevel.Information));
            }

            [Test]
            public void Should_configure_environmentvariables()
            {
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var mp = new ManagedProcess { CreateSystemProcess = _ => new SystemProcess(null, logger, controller) };

                StringDictionary sd = null;
                void Configure(StringDictionary s) => sd = s;

                mp.Start(ProcessFilename, configureEnvironmentVariables: Configure);
                Assert.That(sd, Is.Not.Null);
            }

            [Test]
            public void Should_catch_start_exception_and_stop()
            {
                var controller = Substitute.For<ISystemProcessController>();
                var logger = Substitute.For<IDaprLogger>();
                var mp = new ManagedProcess { CreateSystemProcess = _ => new SystemProcess(null, logger, controller) };
                var ex = new Exception();

                controller.When(x => x.Start()).Do(_ => throw ex);
                mp.Start(ProcessFilename, logger: logger);

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(2));
                Assert.That(loggerCalls[1].Message, Is.EqualTo($"Error starting process {ProcessFilename}"));
                Assert.That(loggerCalls[1].LogLevel, Is.EqualTo(DaprLogLevel.Error));
                Assert.That(loggerCalls[1].Exception, Is.SameAs(ex));

                controller.Received(1).WaitForExit();
            }

#if NETFRAMEWORK
            [Test]
            public void Should_start_and_monitor_real_process()
            {
                var mp = new ManagedProcess();
                var logger = Substitute.For<IDaprLogger>();
                var filename = TestResourceHelper.CompileTestSystemProcessExe();
                var unplannedExit = false;
                mp.OutputDataReceived += (sender, args) => logger.LogInformation(args.Data);
                mp.UnplannedExit += (sender, args) => unplannedExit = true;

                try
                {
                    // Start and check process
                    mp.Start(filename, "ARG1=VAL1 ARG2=VAL2", logger: logger);
                    Assert.That(mp.Id, Is.Not.Null);
                    var p = System.Diagnostics.Process.GetProcessById(mp.Id.Value);
                    Assert.That(p, Is.Not.Null);

                    // Wait for the logger calls
                    var loggerCalls = logger.ReceivedLoggerCalls();
                    var loopCount = 0;
                    while (!loggerCalls.Any(x => x.Message.Contains("Program Started")))
                    {
                        System.Threading.Thread.Sleep(10);
                        loggerCalls = logger.ReceivedLoggerCalls();
                        if (loopCount++ > 100)
                        {
                            // Gave it up to a second to start up, when it should just take a few milliseconds
                            throw new Exception("Process did not start up");
                        }
                    }

                    // Check command line
                    var cmd = mp.GetCommandLine();
                    Assert.That(cmd, Is.Not.Null);

                    // Second start should have no effect (already running)
                    var loggerCallCount = loggerCalls.Length;
                    mp.Start(filename);
                    loggerCalls = logger.ReceivedLoggerCalls();
                    Assert.That(loggerCalls.Length, Is.EqualTo(loggerCallCount));

                    // Make sure we got the arguments call
                    var argsCall = loggerCalls.FirstOrDefault(x => x.Message.StartsWith("ARGS|"));
                    Assert.That(argsCall, Is.Not.Null);
                    Assert.That(argsCall.Message, Is.EqualTo("ARGS|ARG1=VAL1|ARG2=VAL2"));
                }
                finally
                {
                    // Kill it to force an unplanned exit
                    var p = System.Diagnostics.Process.GetProcessById(mp.Id.Value);
                    p.Kill();

                    // Wait for unplanned exit
                    var loopCount = 0;
                    while (!unplannedExit)
                    {
                        System.Threading.Thread.Sleep(10);
                        if (loopCount++ > 100)
                        {
                            // Gave it up to a second to shut down, when it should just take a few milliseconds
                            throw new Exception("Process did not raise unplanned exit");
                        }
                    }

                    // Delete the resource file
                    TestResourceHelper.DeleteTestProcess(filename);
                }
            }
#endif
        }
    }
}
