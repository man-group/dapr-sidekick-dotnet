using System.Collections.Generic;

namespace Dapr.Sidekick.Process
{
    public class MockPortAvailabilityChecker : IPortAvailabilityChecker
    {
        public int GetAvailablePort(int startingPort, IEnumerable<int> reservedPorts = null) => startingPort;
    }
}
