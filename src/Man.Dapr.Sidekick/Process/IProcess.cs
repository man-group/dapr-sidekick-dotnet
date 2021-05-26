using Man.Dapr.Sidekick.Threading;

namespace Man.Dapr.Sidekick.Process
{
    /// <summary>
    /// Represents a system process.
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// Gets the unique process identifier (PID).
        /// </summary>
        int? Id { get; }

        /// <summary>
        /// Gets the process name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the process is running or not.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Stops the running process.
        /// </summary>
        /// <param name="waitForShutdownSeconds">An optional number of seconds to wait for graceful shutdown. If <c>null</c> the process will be terminated immediately.</param>
        /// <param name="cancellationToken">An optional cancellation token. When set this will terminate the process immediately if <paramref name="waitForShutdownSeconds"/> has not yet elapsed.</param>
        void Stop(int? waitForShutdownSeconds, DaprCancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the command-line arguments that were passed to the process during initialization.
        /// </summary>
        /// <returns>A <see cref="IProcessCommandLine"/> instance.</returns>
        IProcessCommandLine GetCommandLine();
    }
}
