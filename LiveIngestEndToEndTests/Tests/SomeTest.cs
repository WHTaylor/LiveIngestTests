using LiveIngestEndToEndTests.Framework;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Tests
{
    public class Tests
    {
        private readonly DataFileCopier _dataFileCopier = new DataFileCopier("ARGUS");
        private readonly DelayedAssert _asserter = new();

        [SetUp]
        public void Setup()
        {
            // Set up any log/queue following stuff if that seems like a good idea
        }

        [Test]
        public void Test1()
        {
            _dataFileCopier.CopyRun("71790");
            _asserter.Success("71790.nxs");
        }
    }
}
