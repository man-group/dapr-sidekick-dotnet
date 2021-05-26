using System;
using Man.Dapr.Sidekick.Threading;

namespace Man.Dapr.Sidekick.Process
{
    public interface IDaprProcessHost<TOptions> : IDaprProcessHost
        where TOptions : Options.DaprProcessOptions
    {
        /// <summary>
        /// Gets the process options used to configure and start up the current running process.
        /// </summary>
        /// <returns>A <typeparamref name="TOptions"/> instance, or <c>null</c> if the process is not currently running.</returns>
        new TOptions GetProcessOptions();
    }

    public partial interface IDaprProcessHost
    {
        /// <summary>
        /// Gets the process options used to configure and start up the current running process.
        /// </summary>
        /// <returns>A <see cref="Options.DaprProcessOptions"/> instance, or <c>null</c> if the process is not currently running.</returns>
        Options.DaprProcessOptions GetProcessOptions();

        /// <summary>
        /// Gets information about the currently running process.
        /// Returns <see cref="DaprProcessInfo.Unknown"/> if the process is not currently running.
        /// </summary>
        /// <returns>A <see cref="DaprProcessInfo"/> instance.</returns>
        DaprProcessInfo GetProcessInfo();

        bool Start(Func<DaprOptions> optionsAccessor, DaprCancellationToken cancellationToken);

        void Stop(DaprCancellationToken cancellationToken);
    }
}
