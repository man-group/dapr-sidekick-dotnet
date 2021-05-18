using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.Logging.Internal
{
    public class DaprColoredConsoleLoggerTests
    {
        public class BeginScope
        {
            [Test]
            public void Should_return_disposable()
            {
                var logger = new DaprColoredConsoleLogger("test-category");
                Assert.That(logger.BeginScope(logger), Is.Not.Null);
            }
        }

        public class IsEnabled
        {
            [Test]
            public void Should_return_options_value()
            {
                var options = new DaprColoredConsoleLoggerOptions();
                var logger = new DaprColoredConsoleLogger("test-category", options);

              Assert.That(logger.IsEnabled(DaprLogLevel.Error), Is.True);
  
                options[DaprLogLevel.Error].Enabled = false;
                Assert.That(logger.IsEnabled(DaprLogLevel.Error), Is.False);
            }
        }

        public class Log
        {
            [Test]
            public void Should_not_write_when_disabled()
            {
                var options = new DaprColoredConsoleLoggerOptions();
                var systemConsole = Substitute.For<ISystemConsole>();
                var logger = new DaprColoredConsoleLogger("test-category", options, systemConsole);

                // First log should be received
                logger.Log(DaprLogLevel.Information, "TEST");
                systemConsole.Received(1).WriteLine(Arg.Any<string>());
                
                // Second log should not when level disabled (still 1 received call)
                options[DaprLogLevel.Information].Enabled = false;
                logger.Log(DaprLogLevel.Information, "TEST2");
                systemConsole.Received(1).WriteLine(Arg.Any<string>());
            }

            [Test]
            public void Should_write_with_default_format()
            {
                var options = new DaprColoredConsoleLoggerOptions();
                var systemConsole = Substitute.For<ISystemConsole>();
                var logger = new DaprColoredConsoleLogger("test-category", options, systemConsole);

                logger.Log(DaprLogLevel.Information, "TEST");
                systemConsole.Received(1).WriteLine(Arg.Is<string>(x =>
                    x.EndsWith("[INF] [test-category] TEST")));
            }

            [Test]
            public void Should_write_with_custom_format()
            {
                var options = new DaprColoredConsoleLoggerOptions();
                var systemConsole = Substitute.For<ISystemConsole>();
                var logger = new DaprColoredConsoleLogger("test-category", options, systemConsole);

                logger.Log(DaprLogLevel.Information, "TEST VALUE {0} AND {1}", "ONE", 300);
                systemConsole.Received(1).WriteLine(Arg.Is<string>(x =>
                    x.EndsWith("[INF] [test-category] TEST VALUE ONE AND 300")));
            }

            [Test]
            public void Should_use_short_log_level()
            {
                var options = new DaprColoredConsoleLoggerOptions();
                var systemConsole = Substitute.For<ISystemConsole>();
                var logger = new DaprColoredConsoleLogger("test-category", options, systemConsole);

                logger.Log(DaprLogLevel.Critical, "TEST");
                logger.Log(DaprLogLevel.Debug, "TEST");
                logger.Log(DaprLogLevel.Error, "TEST");
                logger.Log(DaprLogLevel.Trace, "TEST");
                logger.Log(DaprLogLevel.Warning, "TEST");
                logger.Log(DaprLogLevel.Information, "TEST");

                systemConsole.Received(1).WriteLine(Arg.Is<string>(x => x.Contains("[CRIT]")));
                systemConsole.Received(1).WriteLine(Arg.Is<string>(x => x.Contains("[DBG]")));
                systemConsole.Received(1).WriteLine(Arg.Is<string>(x => x.Contains("[ERR]")));
                systemConsole.Received(1).WriteLine(Arg.Is<string>(x => x.Contains("[TRC]")));
                systemConsole.Received(1).WriteLine(Arg.Is<string>(x => x.Contains("[WRN]")));
                systemConsole.Received(1).WriteLine(Arg.Is<string>(x => x.Contains("[INF]")));
            }

            [Test]
            public void Should_set_foreground_color()
            {
                var options = new DaprColoredConsoleLoggerOptions();
                var systemConsole = Substitute.For<ISystemConsole>();
                var logger = new DaprColoredConsoleLogger("test-category", options, systemConsole);

                var initialColor = System.ConsoleColor.White;
                var logLevelColor = options[DaprLogLevel.Critical].Color;

                systemConsole.ForegroundColor = initialColor; // One call
                logger.Log(DaprLogLevel.Critical, "TEST");

                systemConsole.Received(1).WriteLine(Arg.Is<string>(x => x.Contains("[CRIT]")));
                systemConsole.Received(1).ForegroundColor = logLevelColor;
                systemConsole.Received(2).ForegroundColor = initialColor;
            }
        }
    }
}
