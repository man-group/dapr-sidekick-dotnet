using System.Linq;
using Dapr.Sidekick.Logging;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.Process
{
    public class DaprProcessLoggerTests
    {
        public class ToDaprLogLevel
        {
            [TestCase(null, DaprLogLevel.Information)]
            [TestCase("", DaprLogLevel.Information)]
            [TestCase("unknown", DaprLogLevel.Information)]
            [TestCase("info", DaprLogLevel.Information)]
            [TestCase("debug", DaprLogLevel.Debug)]
            [TestCase("warning", DaprLogLevel.Warning)]
            [TestCase("error", DaprLogLevel.Error)]
            [TestCase("fatal", DaprLogLevel.Critical)]
            public void Should_return_expected_value(string logLevel, DaprLogLevel expected)
            {
                Assert.That(DaprProcessLogger.ToDaprLogLevel(logLevel), Is.EqualTo(expected));
            }
        }

        public class LogData
        {
            [Test]
            public void Should_not_log_empty_data()
            {
                var logger = Substitute.For<IDaprLogger>();
                var processLogger = new DaprProcessLogger(logger, null);
                processLogger.LogData(null);
                processLogger.LogData(string.Empty);
                processLogger.LogData("{ No: \"Data\"}");

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.Zero);
            }

            [Test]
            public void Should_log_invalid_data()
            {
                var logger = Substitute.For<IDaprLogger>();
                var processLogger = new DaprProcessLogger(logger, null);
                processLogger.LogData("THIS_IS_NOT_JSON");
                processLogger.LogData(string.Empty);

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(1));
                Assert.That(loggerCalls[0].Message, Is.EqualTo("THIS_IS_NOT_JSON"));
                Assert.That(loggerCalls[0].LogLevel, Is.EqualTo(DaprLogLevel.Information));
            }

            [TestCase("NO_ACTION", null)]
            [TestCase("dapr initialized. Status: Running. Init Elapsed 243.003ms", DaprProcessStatus.Started)]
            [TestCase("Dapr was initialized and is now running", DaprProcessStatus.Started)]
            [TestCase("placement service started on port 50005", DaprProcessStatus.Started)]
            [TestCase("some placement service Started on port 54321!", DaprProcessStatus.Started)]
            [TestCase("sentry certificate authority is running, protecting ya'll", DaprProcessStatus.Started)]
            [TestCase("your Sentry service is now Running and protecting everything", DaprProcessStatus.Started)]
            [TestCase("stop command issued. Shutting down all operations", DaprProcessStatus.Stopping)]
            [TestCase("stop shutting down", DaprProcessStatus.Stopping)]
            public void Should_interpret_message(string message, DaprProcessStatus? expectedStatus)
            {
                var data = JsonConvert.SerializeObject(new
                {
                    msg = message
                });

                var logger = Substitute.For<IDaprLogger>();
                var processUpdater = Substitute.For<IDaprProcessUpdater>();
                var processLogger = new DaprProcessLogger(logger, processUpdater);
                processLogger.LogData(data);

                if (expectedStatus == null)
                {
                    Assert.That(processUpdater.ReceivedCalls().Count(), Is.Zero);
                }
                else
                {
                    processUpdater.Received(1).UpdateStatus(expectedStatus.Value);
                }
            }

            [Test]
            public void Should_extract_loglevel()
            {
                var data = JsonConvert.SerializeObject(new
                {
                    msg = "MESSAGE",
                    level = "fatal"
                });

                var logger = Substitute.For<IDaprLogger>();
                var processLogger = new DaprProcessLogger(logger, null);
                processLogger.LogData(data);

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(1));
                Assert.That(loggerCalls[0].Message, Is.EqualTo("MESSAGE"));
                Assert.That(loggerCalls[0].LogLevel, Is.EqualTo(DaprLogLevel.Critical));
            }

            [Test]
            public void Should_extract_version()
            {
                var data = JsonConvert.SerializeObject(new
                {
                    msg = "MESSAGE",
                    ver = "VERSION"
                });

                var logger = Substitute.For<IDaprLogger>();
                var processUpdater = Substitute.For<IDaprProcessUpdater>();
                var processLogger = new DaprProcessLogger(logger, processUpdater);
                processLogger.LogData(data);

                processUpdater.Received(1).UpdateVersion("VERSION");
            }
        }
    }
}
