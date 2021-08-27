using System.ServiceModel;
using ICAT4IngestLibrary.org.icatproject.isisicat;
using LiveIngestEndToEndTests.Framework;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Tests
{
    public class Tests
    {
        private readonly DataFileCopier _dataFileCopier = new("EMU");
        private readonly DelayedAssert _asserter = new();

        [SetUp]
        public void Setup()
        {
            // Set up any log/queue following stuff if that seems like a good idea
        }

        [TearDown]
        public void TearDown()
        {
            var c = new CATClient(
                new BasicHttpBinding(BasicHttpSecurityMode.Transport),
                new EndpointAddress(
                    "https://icat-dev.isis.stfc.ac.uk/ICATService/ICAT?wsdl"));
            var creds = new loginEntry[]
            {
                new()
                {
                    key = "username",
                    value = "isisdata@stfc.ac.uk"
                },
                new()
                {
                    key = "password",
                    value = "redacted"
                },
            };
            var sessionId = c.login("uows", creds);
            var i = c.search(
                sessionId,
                "select i from Investigation i where i.name =  'CAL_EMU_21/04/2021 11:42:06' include 1");
            if (i?.Length > 0)
            {
                TestContext.Progress.WriteLine(
                    "No matter what the asserter may think, inv was created. Deleting it");
                c.delete(sessionId, (entityBaseBean) i[0]);
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
