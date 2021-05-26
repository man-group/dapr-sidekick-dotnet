#if NET35
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Threading
{
    public class DaprCancellationTokenTests
    {
        [Test]
        public void Should_return_different_instance_for_none()
        {
            Assert.That(DaprCancellationToken.None, Is.Not.SameAs(DaprCancellationToken.None));
        }

        [Test]
        public void Should_always_return_false_for_cancellation_requested()
        {
            Assert.That(default(DaprCancellationToken).IsCancellationRequested, Is.False);
        }
    }
}
#endif
