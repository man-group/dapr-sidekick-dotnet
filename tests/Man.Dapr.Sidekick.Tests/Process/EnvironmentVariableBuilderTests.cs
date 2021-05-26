using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class EnvironmentVariableBuilderTests
    {
        public class Add
        {
            [Test]
            public void Should_not_add_when_invalid_name()
            {
                var builder = new EnvironmentVariableBuilder();

                Assert.That(builder.Add(null, null), Is.SameAs(builder));
                Assert.That(builder.Add(string.Empty, null), Is.SameAs(builder));
                Assert.That(builder.Add("   ", null), Is.SameAs(builder));
                Assert.That(builder.ToDictionary().Count, Is.Zero);
            }

            [Test]
            public void Should_not_add_when_null_value()
            {
                var builder = new EnvironmentVariableBuilder();

                Assert.That(builder.Add("TEST", null), Is.SameAs(builder));
                Assert.That(builder.ToDictionary().Count, Is.Zero);
            }

            [Test]
            public void Should_add_without_predicate()
            {
                var builder = new EnvironmentVariableBuilder();

                Assert.That(builder.Add("NAME", "VALUE"), Is.SameAs(builder));

                var result = builder.ToDictionary();
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result["NAME"], Is.EqualTo("VALUE"));
            }

            [Test]
            public void Should_not_add_when_predicate_false()
            {
                var builder = new EnvironmentVariableBuilder();

                Assert.That(builder.Add("NAME", "VALUE", () => false), Is.SameAs(builder));
                Assert.That(builder.ToDictionary().Count, Is.Zero);
            }

            [Test]
            public void Should_add_when_predicate_true()
            {
                var builder = new EnvironmentVariableBuilder();

                Assert.That(builder.Add("NAME", "VALUE", () => true), Is.SameAs(builder));

                var result = builder.ToDictionary();
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result["NAME"], Is.EqualTo("VALUE"));
            }

            [Test]
            public void Should_replace_existig_value()
            {
                var builder = new EnvironmentVariableBuilder();

                Assert.That(builder.Add("NAME", "VALUE1", () => true), Is.SameAs(builder));
                Assert.That(builder.Add("NAME", "VALUE2", () => true), Is.SameAs(builder));

                var result = builder.ToDictionary();
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result["NAME"], Is.EqualTo("VALUE2"));
            }
        }

        public class ToDictionary
        {
            [Test]
            public void Should_return_all_values()
            {
                var builder = new EnvironmentVariableBuilder();

                builder.Add("NAME1", "VALUE1");
                builder.Add("NAME2", "VALUE2");
                builder.Add("NAME3", "VALUE3");

                var result = builder.ToDictionary();
                Assert.That(result.Count, Is.EqualTo(3));
                Assert.That(result["NAME1"], Is.EqualTo("VALUE1"));
                Assert.That(result["NAME2"], Is.EqualTo("VALUE2"));
                Assert.That(result["NAME3"], Is.EqualTo("VALUE3"));
            }

            [Test]
            public void Should_create_new_instance_each_call()
            {
                var builder = new EnvironmentVariableBuilder();

                builder.Add("NAME1", "VALUE1");
                var result1 = builder.ToDictionary();

                builder.Add("NAME2", "VALUE2");
                var result2 = builder.ToDictionary();

                Assert.That(result1.Count, Is.EqualTo(1));
                Assert.That(result2.Count, Is.EqualTo(2));
                Assert.That(result1, Is.Not.SameAs(result2));
            }
        }
    }
}
