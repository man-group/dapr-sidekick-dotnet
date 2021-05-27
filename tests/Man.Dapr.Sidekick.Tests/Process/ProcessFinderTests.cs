using System.Linq;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class ProcessFinderTests
    {
        public class FindExistingProcesses
        {
            [Test]
            public void Should_find_by_name()
            {
                // We know at least the current process is running so see if we can find it.
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var finder = new ProcessFinder();
                var result = finder.FindExistingProcesses(process.ProcessName);
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(1));
                Assert.That(result.Any(x => x.Id == process.Id), Is.True);
            }
        }
    }
}
