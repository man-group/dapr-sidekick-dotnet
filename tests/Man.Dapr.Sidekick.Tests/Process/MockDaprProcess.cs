using System.Collections.Generic;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    internal class MockDaprProcess : DaprProcess<MockDaprProcessOptions>
    {
        public const string DefaultName = "Mock";

        public MockDaprProcess()
            : base(DefaultName)
        {
            Options = new MockDaprProcessOptions();
            Arguments = new Dictionary<string, string>();
        }

        public MockDaprProcess(ProcessComparison comparison)
            : this()
        {
            Comparison = comparison;
        }

        public MockDaprProcessOptions Options { get; }

        public ProcessComparison Comparison { get; }

        public IDictionary<string, string> Arguments { get; }

        protected override void AddCommandLineArguments(MockDaprProcessOptions source, CommandLineArgumentBuilder builder)
        {
        }

        protected override void AddEnvironmentVariables(MockDaprProcessOptions source, EnvironmentVariableBuilder builder)
        {
        }

        protected override void AssignLocations(MockDaprProcessOptions options, string daprFolder)
        {
        }

        protected override void AssignPorts(PortAssignmentBuilder<MockDaprProcessOptions> builder)
        {
        }

        protected override ProcessComparison CompareProcessOptions(MockDaprProcessOptions proposedProcessOptions, MockDaprProcessOptions existingProcessOptions, IProcess existingProcess) => Comparison;

        protected override MockDaprProcessOptions GetProcessOptions(DaprOptions daprOptions)
        {
            Options.EnrichFrom(daprOptions);
            return Options;
        }

        protected override void ParseCommandLineArgument(MockDaprProcessOptions target, string name, string value)
        {
            // Make sure argument is in dictionary
            Assert.That(Arguments, Does.ContainKey(name));
            Assert.That(Arguments[name], Is.EqualTo(value));
        }
    }
}
