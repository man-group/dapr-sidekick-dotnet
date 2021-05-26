#if NET35
namespace Man.Dapr.Sidekick.Threading
{
    public partial struct DaprCancellationToken
    {
        /// <summary>
        /// Gets a value indicating whether cancellation has been requested for this token.
        /// </summary>
        /// <value>Whether cancellation has been requested for this token.</value>
        /// <remarks>
        /// <para>
        /// If this property is true, it only guarantees that cancellation has been requested.
        /// It does not guarantee that every registered handler
        /// has finished executing, nor that cancellation requests have finished propagating
        /// to all registered handlers.  Additional synchronization may be required,
        /// particularly in situations where related objects are being canceled concurrently.
        /// </para>
        /// </remarks>
        public bool IsCancellationRequested => false;
    }
}
#endif
