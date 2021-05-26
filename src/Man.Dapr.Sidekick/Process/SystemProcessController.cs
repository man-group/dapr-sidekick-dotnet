namespace Man.Dapr.Sidekick.Process
{
    internal class SystemProcessController : ISystemProcessController
    {
        private readonly System.Diagnostics.Process _process;

        public SystemProcessController(System.Diagnostics.Process process)
        {
            _process = process;
        }

        public void Kill() => _process.Kill();

        public bool Start() => _process.Start();

        public void WaitForExit() => _process.WaitForExit();

        public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);
    }
}
