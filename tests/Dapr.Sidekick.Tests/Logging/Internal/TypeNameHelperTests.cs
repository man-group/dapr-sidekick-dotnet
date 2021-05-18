using NUnit.Framework;

namespace Dapr.Sidekick.Logging.Internal
{
    public class TypeNameHelperTests
    {
        public class GetTypeDisplayName
        {
            [Test]
            public void Should_get_for_custom_type()
            {
                Assert.That(TypeNameHelper.GetTypeDisplayName(typeof(IDaprLoggerFactory)), Is.EqualTo("Dapr.Sidekick.Logging.IDaprLoggerFactory"));
            }

            [Test]
            public void Should_get_for_nested_type()
            {
                Assert.That(TypeNameHelper.GetTypeDisplayName(typeof(DaprColoredConsoleLoggerOptions.LogLevelOptions)), Is.EqualTo("Dapr.Sidekick.Logging.DaprColoredConsoleLoggerOptions.LogLevelOptions"));
            }

            [Test]
            public void Should_get_for_generic_type()
            {
                Assert.That(TypeNameHelper.GetTypeDisplayName(typeof(IDaprLogger<DaprDisposable>)), Is.EqualTo("Dapr.Sidekick.Logging.IDaprLogger"));
            }

            [Test]
            public void Should_get_for_builtin_type()
            {
                Assert.That(TypeNameHelper.GetTypeDisplayName(typeof(string)), Is.EqualTo("string"));
            }

        }
    }
}
