using LiveIngestEndToEndTests.Framework;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Tests
{
    public class Tests
    {
        private readonly DataFileCopier _dataFileCopier = new DataFileCopier().ForInstrument("ARGUS");

        [SetUp]
        public void Setup()
        {
            // Set up any log/queue following stuff if that seems like a good idea
        }

        [Test]
        public void Test1()
        {
            _dataFileCopier.CopyRun("71790");
            Assert.That(2, Is.EqualTo(1 + 1).After(5).Seconds);
        }
    }
}
