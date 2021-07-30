using NUnit.Framework;

namespace LiveIngestEndToEndTests
{
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public void EnvironmentSetup()
        {
            // Validate things are running/available to run
            var toWatch = new DataArchiverCreator().CreateDataArchive();
            // Setup ICAT data? AKA delete leftovers in advance
            // Queue component setup/maybe clearing?
            // Startup live ingest process(es)
        }
    }
}