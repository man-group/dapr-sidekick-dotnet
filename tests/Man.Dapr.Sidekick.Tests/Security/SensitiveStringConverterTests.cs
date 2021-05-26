using System;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Security
{
    public class SensitiveStringConverterTests
    {
        [Test]
        public void Should_convert_from_string_only()
        {
            var converter = new SensitiveStringConverter();

            Assert.That(converter.CanConvertFrom(typeof(string)), Is.True);
            Assert.That(converter.CanConvertFrom(typeof(int)), Is.False);
            Assert.That(converter.CanConvertFrom(typeof(object)), Is.False);

            Assert.That(converter.ConvertFrom("TEST"), Is.EqualTo(new SensitiveString("TEST")));
            Assert.Throws(Is.InstanceOf<NotSupportedException>(), () => converter.ConvertFrom(100));
        }
    }
}
