using System;
using NUnit.Framework;

namespace Dapr.Sidekick
{
    [TestFixture]
    public class DaprDisposableTests
    {
        [Test]
        public void Should_dispose()
        {
            var d = new MockDisposable();
            Assert.That(d.IsDisposed, Is.False);
            Assert.That(d.IsDisposing, Is.False);

            d.Dispose();
            Assert.That(d.IsDisposing, Is.False);
            Assert.That(d.IsDisposed, Is.True);
            Assert.That(d.IsDisposingValue, Is.True);
            Assert.That(d.OnDisposingValue, Is.True);

            d.IsDisposingValue = null;
            d.OnDisposingValue = null;
            d.Dispose();
            Assert.That(d.IsDisposing, Is.False);
            Assert.That(d.IsDisposed, Is.True);
            Assert.That(d.IsDisposingValue, Is.Null);
            Assert.That(d.OnDisposingValue, Is.Null);
        }

        [Test]
        public void Should_dispose_from_finalizer()
        {
            var d = new MockDisposable();

            d.InvokeDispose(false);
            Assert.That(d.IsDisposing, Is.False);
            Assert.That(d.IsDisposed, Is.True);
            Assert.That(d.IsDisposingValue, Is.True);
            Assert.That(d.OnDisposingValue, Is.False);

            d.IsDisposingValue = null;
            d.OnDisposingValue = null;
            d.InvokeDispose(false);
            Assert.That(d.IsDisposing, Is.False);
            Assert.That(d.IsDisposed, Is.True);
            Assert.That(d.IsDisposingValue, Is.Null);
            Assert.That(d.OnDisposingValue, Is.Null);
        }

        [Test]
        public void Should_ensure_not_disposed()
        {
            var d = new MockDisposable();
            Assert.DoesNotThrow(() => d.InvokeEnsureNotDisposed());
            d.Dispose();
            Assert.Throws<ObjectDisposedException>(() => d.InvokeEnsureNotDisposed());
        }
    }
}
