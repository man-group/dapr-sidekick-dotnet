using System;

namespace Dapr.Sidekick
{
    /// <summary>
    /// Simple disposable object suitable for use as a base class for other disposable objects.
    /// </summary>
    public class DaprDisposable : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is currently being disposed.
        /// </summary>
        public bool IsDisposing { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed || IsDisposing)
            {
                return;
            }

            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of this instance. Finalizers should invoke this method with <paramref name="disposing"/> set to <c>false</c>.
        /// </summary>
        /// <param name="disposing"><c>true</c> if invoked via the Dispose method, else <c>false</c> if invoked by a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed || IsDisposing)
            {
                return;
            }

            try
            {
                IsDisposing = true;
                OnDisposing(disposing);
            }
            finally
            {
                IsDisposing = false;
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Called when this instance is being disposed. Inheritors should override this instance to deterministically release both managed and unmanaged resources.
        /// Managed objects should be freed when <paramref name="disposing"/> is <c>true</c>, unmanaged resouces should be freed whenever the method is invoked.
        /// </summary>
        /// <param name="disposing"><c>true</c> if invoked via the Dispose method, else <c>false</c> if invoked by a finalizer.</param>
        protected virtual void OnDisposing(bool disposing)
        {
        }

        /// <summary>
        /// Throws a <see cref="ObjectDisposedException"/> if this instance has been disposed.
        /// </summary>
        protected void EnsureNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
