using System.Collections.Generic;

namespace Dapr.Sidekick.Process
{
    public interface IProcessFinder
    {
        IEnumerable<IProcess> FindExistingProcesses(string processName);
    }
}
