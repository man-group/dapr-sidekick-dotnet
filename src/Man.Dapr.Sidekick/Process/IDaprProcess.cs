using System;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Threading;

namespace Man.Dapr.Sidekick.Process
{
    public interface IDaprProcess<TOptions>
        where TOptions : Options.DaprProcessOptions
    {
        /// <summary>
        /// Gets the options that were actually used to successfully start the process the last time it was launched.
        /// This will include all calculated file paths and auto-assigned ports.
        /// </summary>
        TOptions LastSuccessfulOptions { get; }

        DaprProcessInfo GetProcessInfo();

        bool Start(Func<DaprOptions> optionsAccessor, IDaprLogger logger, DaprCancellationToken cancellationToken = default);

        void Stop(DaprCancellationToken cancellationToken = default);

        event EventHandler<DaprProcessStartingEventArgs<TOptions>> Starting;

        event EventHandler<DaprProcessStoppingEventArgs> Stopping;
    }
}
