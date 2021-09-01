using ICAT4IngestLibrary.org.icatproject.isisicat;
using ICAT4IngestLibrary;
using LiveIngestEndToEndTests.Framework;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Tests
{
    public class Tests
    {
        private readonly DataFileCopier _dataFileCopier = new("EMU");
        private readonly DelayedAssert _asserter = new();
        private static readonly ICATClient _icatClient = Framework.ICAT.Client;

        [SetUp]
        public void Setup()
        {
            // Set up any log/queue following stuff if that seems like a good idea
            var i = _icatClient.Service.search(
                _icatClient.SessionId,
                "select i from Investigation i where i.name =  'CAL_EMU_21/04/2021 11:42:06' include 1");
            if (i?.Length > 0)
            {
                TestContext.Progress.WriteLine(
                    "No matter what the asserter may think, inv was created. Deleting it");
                _icatClient.Service.delete(_icatClient.SessionId, (entityBaseBean) i[0]);
            }
        }

        [TearDown]
        public void TearDown()
        {
            var i = _icatClient.Service.search(
                _icatClient.SessionId,
                "select i from Investigation i where i.name =  'CAL_EMU_21/04/2021 11:42:06' include 1");
            if (i?.Length > 0)
            {
                TestContext.Progress.WriteLine(
                    "No matter what the asserter may think, inv was created. Deleting it");
                _icatClient.Service.delete(_icatClient.SessionId, (entityBaseBean) i[0]);
            }
        }

        [Test]
        public void Test1()
        {
            _dataFileCopier.CopyRun("112486");
            _asserter.Success("112486.xml");
        }
    }
}
