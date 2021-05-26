using System;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Threading;

namespace Man.Dapr.Sidekick.Process
{
    public class SystemProcess : IProcess
    {
        private readonly IDaprLogger _logger;
        private readonly ISystemProcessController _controller;

        internal SystemProcess(System.Diagnostics.Process process, IDaprLogger logger = null, ISystemProcessController controller = null)
        {
            Process = process;
            _logger = logger;
            _controller = controller ?? new SystemProcessController(process);
        }

        public int? Id => Process?.Id;

        public string Name => Process?.ProcessName;

        public bool IsRunning => Process?.HasExited == false;

        internal System.Diagnostics.Process Process { get; }

        public IProcessCommandLine GetCommandLine() => new ProcessCommandLine(this);

        public void Stop(int? waitForShutdownSeconds, DaprCancellationToken cancellationToken = default)
        {
            var graceful = false;
            if (waitForShutdownSeconds.HasValue && waitForShutdownSeconds > 0)
            {
                try
                {
                    _logger?.LogInformation("Waiting {ProcessWaitSeconds} second(s) for {ProcessName} to stop...", waitForShutdownSeconds, Name);
                    var waitUntil = DateTime.Now.AddSeconds(waitForShutdownSeconds.Value);
                    do
                    {
                        // Wait for the process to stop on its own until cancellation requested.
                        if (_controller.WaitForExit(100))
                        {
                            graceful = true;
                            break;
                        }
                    }
                    while (IsRunning && !cancellationToken.IsCancellationRequested && DateTime.Now < waitUntil);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error stopping Process");
                }
            }
            else if (!IsRunning)
            {
                try
                {
                    // Already exited. Use WaitForExit() with no timeout (IMPORTANT!) to flush all existing stdout messages.
                    _controller.WaitForExit();
                    graceful = true;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error waiting for Process to exit");
                }
            }

            if (!graceful)
            {
                try
                {
                    // Kill it
                    _logger?.LogInformation("Killing Process {ProcessName} PID:{ProcessId}...", Name, Id);
                    _controller.Kill();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error killing Process");
                }
            }
        }

        internal void Start()
        {
            _controller.Start();
            Native.NativeChildProcessTracker.AddProcess(Process); // Ensure the tracked process shuts down when this process shuts down
        }
    }
}
