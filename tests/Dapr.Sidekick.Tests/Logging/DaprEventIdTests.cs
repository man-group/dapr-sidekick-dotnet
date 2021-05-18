using NUnit.Framework;

namespace Dapr.Sidekick.Logging
{
    public class DaprEventIdTests
    {
        [Test]
        public void Should_implement_equality()
        {
            Assert.That((DaprEventId)100, Is.EqualTo(new DaprEventId(100)));
            Assert.That((DaprEventId)100 == (DaprEventId)100, Is.True);
            Assert.That(new DaprEventId(200) == (DaprEventId)100, Is.False);
            Assert.That(new DaprEventId(200) != (DaprEventId)100, Is.True);
            Assert.That(new DaprEventId(200).Equals((DaprEventId)200), Is.True);
            Assert.That(new DaprEventId(200).Equals(null), Is.False);
            Assert.That((DaprEventId)200, Is.Not.EqualTo(new DaprEventId(100)));
            Assert.That(new DaprEventId(100).GetHashCode(), Is.EqualTo(100));
        }

        [Test]
        public void Should_set_id_and_name()
        {
            var value = new DaprEventId(100, "TEST");
            Assert.That(value.Id, Is.EqualTo(100));
            Assert.That(value.Name, Is.EqualTo("TEST"));
        }

        [Test]
        public void Should_override_tostring()
        {
            Assert.That(new DaprEventId(100).ToString(), Is.EqualTo("100"));
            Assert.That(new DaprEventId(100, "TEST").ToString(), Is.EqualTo("TEST"));
        }
    }
}
