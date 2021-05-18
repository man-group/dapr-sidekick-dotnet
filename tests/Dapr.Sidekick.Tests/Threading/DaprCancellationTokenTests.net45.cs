#if !NET35
using System.Threading;
using NUnit.Framework;

namespace Dapr.Sidekick.Threading
{
    public class DaprCancellationTokenTests
    {
        [Test]
        public void Should_return_different_instance_for_none()
        {
            Assert.That(DaprCancellationToken.None, Is.Not.SameAs(DaprCancellationToken.None));
        }

        [Test]
        public void Should_wrap_cancellation_token()
        {
            var cts = new CancellationTokenSource();
            var ct = new DaprCancellationToken(cts.Token);

            Assert.That(ct.IsCancellationRequested, Is.False);
            Assert.That(ct.CancellationToken, Is.EqualTo(cts.Token));

            cts.Cancel();
            Assert.That(ct.IsCancellationRequested, Is.True);
        }
    }
}
#endif
