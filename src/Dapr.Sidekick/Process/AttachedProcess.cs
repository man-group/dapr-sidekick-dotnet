using Dapr.Sidekick.Threading;

namespace Dapr.Sidekick.Process
{
    internal class AttachedProcess : IProcess
    {
        private IProcess _systemProcess;

        public AttachedProcess(IProcess systemProcess)
        {
            _systemProcess = systemProcess;
        }

        public int? Id => _systemProcess?.Id;

        public string Name => _systemProcess?.Name;

        public bool IsRunning => _systemProcess?.IsRunning ?? false;

        public IProcessCommandLine GetCommandLine() => _systemProcess?.GetCommandLine();

        public void Stop(int? waitForShutdownSeconds = null, DaprCancellationToken cancellationToken = default)
        {
            // Just detach
            _systemProcess = null;
        }
    }
}
