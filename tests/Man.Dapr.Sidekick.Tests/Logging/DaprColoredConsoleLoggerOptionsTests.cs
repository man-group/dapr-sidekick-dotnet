using System;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Logging
{
    public class DaprColoredConsoleLoggerOptionsTests
    {
        public class Constructor
        {
            [Test]
            public void Should_set_defaults()
            {
                var options = new DaprColoredConsoleLoggerOptions();

                Assert.That(options[DaprLogLevel.Information].LogLevel, Is.EqualTo(DaprLogLevel.Information)); // Only need to test this property once
                Assert.That(options[DaprLogLevel.Information].Enabled, Is.True);
                Assert.That(options[DaprLogLevel.Information].Color, Is.EqualTo(ConsoleColor.Cyan));

                Assert.That(options[DaprLogLevel.Trace].Enabled, Is.True);
                Assert.That(options[DaprLogLevel.Trace].Color, Is.EqualTo(ConsoleColor.DarkGreen));

                Assert.That(options[DaprLogLevel.Debug].Enabled, Is.True);
                Assert.That(options[DaprLogLevel.Debug].Color, Is.EqualTo(ConsoleColor.Green));

                Assert.That(options[DaprLogLevel.Warning].Enabled, Is.True);
                Assert.That(options[DaprLogLevel.Warning].Color, Is.EqualTo(ConsoleColor.Yellow));

                Assert.That(options[DaprLogLevel.Error].Enabled, Is.True);
                Assert.That(options[DaprLogLevel.Error].Color, Is.EqualTo(ConsoleColor.Red));

                Assert.That(options[DaprLogLevel.Critical].Enabled, Is.True);
                Assert.That(options[DaprLogLevel.Critical].Color, Is.EqualTo(ConsoleColor.DarkRed));
            }
        }

        public class Get
        {
            [Test]
            public void Should_reuse_instance_for_same_loglevel()
            {
                var options = new DaprColoredConsoleLoggerOptions();
                Assert.That(options.Get(DaprLogLevel.Error), Is.SameAs(options.Get(DaprLogLevel.Error)));
            }
        }
    }
}
