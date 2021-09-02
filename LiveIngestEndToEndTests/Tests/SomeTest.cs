using ICAT4IngestLibrary.org.icatproject.isisicat;
using ICAT4IngestLibrary;
using LiveIngestEndToEndTests.Framework;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Tests
{
    public class Tests
    {
        private readonly DataFileCopier _dataFileCopier = new("EMU");
        private readonly MessageAsserter _messageAsserter = new();
        private static readonly ICATClient _icatClient = Framework.ICAT.Client;
        private const string _expectedFileName = "CAL_EMU_21/04/2021 11:42:06";

        [TearDown]
        public void TearDown()
        {
            var i = _icatClient.Service.search(
                _icatClient.SessionId,
                $"select i from Investigation i " +
                $"where i.name =  '{_expectedFileName}' include 1");

            if (i?.Length > 0)
            {
                _icatClient.Service.delete(_icatClient.SessionId, (entityBaseBean) i[0]);
            }
        }

        [Test]
        public void Test1()
        {
            _dataFileCopier.CopyRun("112486");
            _messageAsserter.ReachesSuccess("112486.xml");
            var i = _icatClient.Service.search(
                _icatClient.SessionId,
                $"select i from Investigation i " +
                $"where i.name =  '{_expectedFileName}' include 1");
            Assert.AreEqual(1, i?.Length,
                $"The expected file '{_expectedFileName}' was not in ICAT");
        }
    }
}
