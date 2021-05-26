using NUnit.Framework;

namespace Man.Dapr.Sidekick.Http
{
    public class UriHelperTests
    {
        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("http://localhost", 80)]
        [TestCase("http://bob@test*:1234", 1234)]
        [TestCase("http://*:2345", 2345)]
        [TestCase("http://+:3456", 3456)]
        [TestCase("http://+:4567/something?else", 4567)]
        [TestCase("https://www.something.com:5678", 5678)]
        public void Should_parse_uri(string uri, int? expected)
        {
            Assert.That(UriHelper.Parse(uri)?.Port, Is.EqualTo(expected));
        }
    }
}
