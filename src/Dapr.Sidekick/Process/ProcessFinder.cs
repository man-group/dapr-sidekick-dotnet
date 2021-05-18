using System.Collections.Generic;
using System.Linq;

namespace Dapr.Sidekick.Process
{
    internal class ProcessFinder : IProcessFinder
    {
        public IEnumerable<IProcess> FindExistingProcesses(string processName) =>
            System.Diagnostics.Process.GetProcessesByName(processName).Select(x => (IProcess)new SystemProcess(x));
    }
}
