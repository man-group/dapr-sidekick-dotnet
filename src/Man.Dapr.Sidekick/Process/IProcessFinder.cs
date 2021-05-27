using System.Collections.Generic;

namespace Man.Dapr.Sidekick.Process
{
    public interface IProcessFinder
    {
        IEnumerable<IProcess> FindExistingProcesses(string processName);
    }
}
