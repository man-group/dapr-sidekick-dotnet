using NUnit.Framework;

namespace Dapr.Sidekick.Process
{
    public class CommandLineArgumentBuilderTests
    {
        public class Constructor
        {
            [Test]
            public void Should_use_default_prefix()
            {
                var builder = new CommandLineArgumentBuilder();
                Assert.That(builder.ArgumemtPrefix, Is.EqualTo("--"));
                Assert.That(builder.ArgumentSeparator, Is.EqualTo(" "));
            }

            [Test]
            public void Should_use_specified_prefix()
            {
                var builder = new CommandLineArgumentBuilder("TEST");
                Assert.That(builder.ArgumemtPrefix, Is.EqualTo("TEST"));
                Assert.That(builder.ArgumentSeparator, Is.EqualTo(" "));
            }
        }

        public class Add
        {
            [Test]
            public void Should_not_add_empty_name()
            {
                var builder = new CommandLineArgumentBuilder();
                Assert.That(builder.Add(null), Is.SameAs(builder));
                Assert.That(builder.Add(string.Empty), Is.SameAs(builder));
                Assert.That(builder.Add("  "), Is.SameAs(builder));
                Assert.That(builder.ToString(), Is.Empty);
            }

            [Test]
            public void Should_use_specified_prefix()
            {
                var builder = new CommandLineArgumentBuilder("**PREFIX**");
                builder.Add("NAME", "VALUE");
                Assert.That(builder.ToString(), Is.EqualTo("**PREFIX**NAME VALUE"));
            }

            [Test]
            public void Should_use_specified_separator()
            {
                var builder = new CommandLineArgumentBuilder();
                builder.ArgumentSeparator = "=";
                builder.Add("NAME", "VALUE");
                Assert.That(builder.ToString(), Is.EqualTo("--NAME=VALUE"));
            }

            [Test]
            public void Should_not_add_duplicate_prefix()
            {
                var builder = new CommandLineArgumentBuilder();
                builder.Add("--NAME", "VALUE");
                Assert.That(builder.ToString(), Is.EqualTo("--NAME VALUE"));
            }

            [Test]
            public void Should_not_add_when_predicate_false()
            {
                var builder = new CommandLineArgumentBuilder();
                Assert.That(builder.Add("NAME", "VALUE", predicate: () => false), Is.SameAs(builder));
                Assert.That(builder.ToString(), Is.Empty);
            }

            [Test]
            public void Should_add_boolean_without_value()
            {
                var builder = new CommandLineArgumentBuilder();
                builder.Add("NAME", true);
                Assert.That(builder.ToString(), Is.EqualTo("--NAME"));
            }

            [Test]
            public void Should_add_quotes_when_value_contains_separator()
            {
                var builder = new CommandLineArgumentBuilder();
                builder.Add("NAME", "WITH SEPARATOR");
                Assert.That(builder.ToString(), Is.EqualTo("--NAME \"WITH SEPARATOR\""));
            }

            [Test]
            public void Should_add_quotes_when_forced()
            {
                var builder = new CommandLineArgumentBuilder();
                builder.Add("NAME", "VALUE", forceQuotes: true);
                Assert.That(builder.ToString(), Is.EqualTo("--NAME \"VALUE\""));
            }

            [Test]
            public void Should_not_add_when_value_null()
            {
                var builder = new CommandLineArgumentBuilder();
                builder.Add("NAME", null);
                Assert.That(builder.ToString(), Is.Empty);
            }

            [Test]
            public void Should_add_when_value_null_and_requiresvalue_false()
            {
                var builder = new CommandLineArgumentBuilder();
                builder.Add("NAME", null, requiresValue: false);
                Assert.That(builder.ToString(), Is.EqualTo("--NAME"));
            }

            [Test]
            public void Should_join_multiple_values()
            {
                var builder = new CommandLineArgumentBuilder();
                builder.Add("NAME1", "VALUE1");
                builder.Add("NAME2", "VALUE2", forceQuotes: true);
                builder.Add("NAME3", true);
                builder.Add("NAME4", 100);
                var result = builder.ToString();

                Assert.That(result, Is.EqualTo("--NAME1 VALUE1 --NAME2 \"VALUE2\" --NAME3 --NAME4 100"));
            }
        }
    }
}
