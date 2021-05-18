using System;
using Dapr.Sidekick.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.Extensions.Logging
{
    public class DaprLoggerTests
    {
        public class Constructor
        {
            [Test]
            public void Should_throw_exception_when_null_logger()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("logger"),
                    () => new DaprLogger(null));
            }
        }

        public class BeginScope
        {
            [Test]
            public void Should_delegate_call_to_underlying_logger()
            {
                var logger = Substitute.For<ILogger>();
                var disposable = Substitute.For<IDisposable>();
                logger.BeginScope(Arg.Any<int>()).Returns(disposable);
                var daprLogger = new DaprLogger(logger);

                var scope = daprLogger.BeginScope(100);
                logger.Received(1).BeginScope(100);
                Assert.That(scope, Is.SameAs(disposable));
            }
        }

        public class IsEnabled
        {
            [Test]
            public void Should_delegate_call_to_underlying_logger()
            {
                var logger = Substitute.For<ILogger>();
                var daprLogger = new DaprLogger(logger);

                foreach (DaprLogLevel daprLogLevel in Enum.GetValues(typeof(DaprLogLevel)))
                {
                    Assert.That(daprLogger.IsEnabled(daprLogLevel), Is.False);
                    logger.IsEnabled((LogLevel)daprLogLevel).Returns(true);
                    Assert.That(daprLogger.IsEnabled(daprLogLevel), Is.True);
                }
            }
        }

        public class Log
        {
            [Test]
            public void Should_delegate_call_to_underlying_logger()
            {
                var logger = Substitute.For<ILogger>();
                var daprLogger = new DaprLogger(logger);
                var ex = new Exception();
                Func<int, Exception, string> formatter = (v, e) => v.ToString() + ":" + e?.ToString();

                daprLogger.Log(DaprLogLevel.Critical, 100, 200, ex, formatter);
                logger.Received(1).Log(LogLevel.Critical, 100, 200, ex, formatter);
            }
        }
    }
}
