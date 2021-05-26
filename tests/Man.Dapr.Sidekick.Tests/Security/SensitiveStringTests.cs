using NUnit.Framework;

namespace Man.Dapr.Sidekick.Security
{
    public class SensitiveStringTests
    {
        [Test]
        public void Should_convert_to_from_string()
        {
            var value = (SensitiveString)"VALUE";
            Assert.That(value.Value, Is.EqualTo("VALUE"));
            Assert.That((string)value, Is.EqualTo("VALUE"));
        }

        [Test]
        public void Should_check_equality()
        {
            Assert.That(new SensitiveString("ONE").Equals(null), Is.False);
            Assert.That(new SensitiveString("ONE").Equals(100), Is.False);

            Assert.That(new SensitiveString("ONE").Equals("ONE"), Is.True);
            Assert.That(new SensitiveString("ONE").Equals(new SensitiveString("ONE")), Is.True);
            Assert.That(new SensitiveString("ONE").Equals("TWO"), Is.False);
            Assert.That(new SensitiveString("ONE").Equals(new SensitiveString("TWO")), Is.False);

            Assert.That(new SensitiveString("ONE") == "ONE", Is.True);
            Assert.That(new SensitiveString("ONE") == new SensitiveString("ONE"), Is.True);
            Assert.That(new SensitiveString("ONE") == "TWO", Is.False);
            Assert.That(new SensitiveString("ONE") == new SensitiveString("TWO"), Is.False);

            Assert.That(new SensitiveString("ONE").GetHashCode(), Is.EqualTo("ONE".GetHashCode()));
        }

        [Test]
        public void Should_override_tostring()
        {
            Assert.That(new SensitiveString(null).ToString(), Is.Null);
            Assert.That(new SensitiveString(string.Empty).ToString(), Is.EqualTo(string.Empty));
            Assert.That(new SensitiveString("  ").ToString(), Is.EqualTo("  "));
            Assert.That(new SensitiveString("VALUE").ToString(), Is.EqualTo("[sensitive]"));
        }
    }
}
