#if !NET35
// Based on https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/Threading/CancellationToken.cs

using System.Threading;

namespace Dapr.Sidekick.Threading
{
    public partial struct DaprCancellationToken
    {
        private readonly CancellationToken _cancellationToken;

        public DaprCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets a value indicating whether cancellation has been requested for this token.
        /// </summary>
        /// <value>Whether cancellation has been requested for this token.</value>
        /// <remarks>
        /// <para>
        /// This property indicates whether cancellation has been requested for this token,
        /// either through the token initially being constructed in a canceled state, or through
        /// calling <see cref="System.Threading.CancellationTokenSource.Cancel()">Cancel</see>
        /// on the token's associated <see cref="CancellationTokenSource"/>.
        /// </para>
        /// <para>
        /// If this property is true, it only guarantees that cancellation has been requested.
        /// It does not guarantee that every registered handler
        /// has finished executing, nor that cancellation requests have finished propagating
        /// to all registered handlers.  Additional synchronization may be required,
        /// particularly in situations where related objects are being canceled concurrently.
        /// </para>
        /// </remarks>
        public bool IsCancellationRequested => _cancellationToken.IsCancellationRequested;

        public CancellationToken CancellationToken => _cancellationToken;
    }
}
#endif
