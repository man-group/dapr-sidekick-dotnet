#pragma warning disable SA1649 // File name should match first type name
using System;
using Dapr.Sidekick.Threading;

namespace Dapr.Sidekick.Process
{
    public class DaprProcessStartingEventArgs<TOptions> : EventArgs
    {
        public DaprProcessStartingEventArgs(TOptions options)
        {
            Options = options;
        }

        public TOptions Options { get; }
    }

    public class DaprProcessStoppingEventArgs : EventArgs
    {
        public DaprProcessStoppingEventArgs(DaprCancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        public DaprCancellationToken CancellationToken { get; }
    }
}
#pragma warning restore SA1649 // File name should match first type name
