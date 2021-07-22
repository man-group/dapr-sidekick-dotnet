using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class ProcessCommandLineTests
    {
        public class Constructor
        {
            [Test]
            public void Should_retrieve_commandline()
            {
                var process = new SystemProcess(System.Diagnostics.Process.GetCurrentProcess());
                var cmd = new ProcessCommandLine(process);
                Assert.That(cmd, Is.Not.Null);
                Assert.That(cmd.Process, Is.SameAs(process));

                if (DaprConstants.IsWindows)
                {
                    // Command line should always contain process EXE name
                    Assert.That(cmd.CommandLine, Is.Not.Null);
                    Assert.That(cmd.Arguments, Is.Not.Empty);
                }
                else
                {
                    // On non-windows platforms everything is empty
                    Assert.That(cmd.CommandLine, Is.Empty);
                    Assert.That(cmd.Arguments, Is.Empty);
                }

                var arguments = cmd.GetArgumentsAsDictionary();
                Assert.That(arguments, Is.Not.Null);
            }
        }

        public class GetArgumentsAsDictionary
        {
            [Test]
            public void Should_return_empty_when_no_arguments()
            {
                var process = Substitute.For<IProcess>();
                var cmd = new ProcessCommandLine(process, "CMD", null);
                var result = cmd.GetArgumentsAsDictionary();
                Assert.That(result, Is.Empty);
            }

            [Test]
            public void Should_replace_existing_arg()
            {
                var arguments = new[]
                {
                    "-Name1",
                    "Value1",
                    "-Name1",
                    "Value2"
                };

                var process = Substitute.For<IProcess>();
                var cmd = new ProcessCommandLine(process, "CMD", arguments);

                var result = cmd.GetArgumentsAsDictionary();
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result["Name1"], Is.EqualTo("Value2"));
            }

            [Test]
            public void Should_get_expected_args()
            {
                var arguments = new[]
                {
                    "-Name1",
                    "---Name2",
                    " Value2 ",
                    "-Name3",
                    "\"Value 3\""
                };

                var process = Substitute.For<IProcess>();
                var cmd = new ProcessCommandLine(process, "CMD", arguments);

                var result = cmd.GetArgumentsAsDictionary();
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Count, Is.EqualTo(3));
                Assert.That(result["Name1"], Is.Null);
                Assert.That(result["Name2"], Is.EqualTo(" Value2 "));
                Assert.That(result["Name3"], Is.EqualTo("\"Value 3\""));
            }
        }

        public class ToStringMethod
        {
            [Test]
            public void Should_return_commandline()
            {
                var cmd = new ProcessCommandLine(null, "COMMAND_LINE", null);
                Assert.That(cmd.ToString(), Is.EqualTo("COMMAND_LINE"));
            }

            [Test]
            public void Should_return_processname()
            {
                var process = Substitute.For<IProcess>();
                process.Name.Returns("PROCESS_NAME");
                var cmd = new ProcessCommandLine(process, null, null);
                Assert.That(cmd.ToString(), Is.EqualTo("PROCESS_NAME"));
            }
        }
    }
}
