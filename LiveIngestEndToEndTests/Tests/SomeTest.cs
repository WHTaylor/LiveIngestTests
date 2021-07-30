using NUnit.Framework;

namespace LiveIngestEndToEndTests.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            // Set up any log/queue following stuff if that seems like a good idea
        }

        [Test]
        public void Test1()
        {
            Assert.AreEqual(2, 1 + 1);
        }
    }
}