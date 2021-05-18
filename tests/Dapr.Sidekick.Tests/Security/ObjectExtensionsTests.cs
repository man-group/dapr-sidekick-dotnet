using NUnit.Framework;

namespace Dapr.Sidekick.Security
{
    public class ObjectExtensionsTests
    {
        public class SensitiveValue
        {
            [Test]
            public void Should_return_null_for_null_value()
            {
                Assert.That(((object)null).SensitiveValue, Is.Null);
            }

            [Test]
            public void Should_return_value_for_value()
            {
                Assert.That("NOT_SENSITIVE".SensitiveValue, Is.EqualTo("NOT_SENSITIVE"));
            }

            [Test]
            public void Should_return_value_for_sensitive_value()
            {
                var sensitiveValue = new SensitiveString("SENSITIVE");
                Assert.That(((object)sensitiveValue).SensitiveValue, Is.EqualTo("SENSITIVE"));
            }
        }
    }
}
