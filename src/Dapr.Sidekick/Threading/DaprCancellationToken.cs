namespace Dapr.Sidekick.Threading
{
    public readonly partial struct DaprCancellationToken
    {
        /// <summary>
        /// Gets an empty CancellationToken value.
        /// </summary>
        /// <remarks>
        /// The <see cref="DaprCancellationToken"/> value returned by this property will be non-cancelable by default.
        /// </remarks>
        public static DaprCancellationToken None => default;
    }
}
