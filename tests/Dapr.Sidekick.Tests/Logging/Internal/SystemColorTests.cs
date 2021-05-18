using System;
using NUnit.Framework;

namespace Dapr.Sidekick.Logging.Internal
{
    public class SystemColorTests
    {
        public class ForegroundColor
        {
            [Test]
            public void Should_get_and_set_foreground_color()
            {
                var systemConsole = new SystemConsole();

                Assert.DoesNotThrow(() =>
                {
                    var initialColor = systemConsole.ForegroundColor;
                    systemConsole.ForegroundColor = ConsoleColor.DarkYellow;
                    systemConsole.ForegroundColor = initialColor;
                });
            }
        }

        public class WriteLine
        {
            [Test]
            public void Should_not_throw_exception()
            {
                var systemConsole = new SystemConsole();

                Assert.DoesNotThrow(() => systemConsole.WriteLine("TEST"));
            }
        }
    }
}
