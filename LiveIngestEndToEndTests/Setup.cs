using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace LiveIngestEndToEndTests
{
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public void EnvironmentSetup()
        {
            var config = new ConfigurationBuilder()
                .AddIniFile("appsettings.ini")
                .Build();

            var exes = config
                .GetSection("executables")
                .GetChildren()
                .Select(c => c.Value);

            foreach (var exe in exes)
            {
                if (File.Exists(exe)) continue;
                SetupError($"Required exe {exe} does not exist");
            }

            // Validate things are running/available to run
            var toWatch = new DataArchiverCreator().CreateDataArchive();
            // Setup ICAT data? AKA delete leftovers in advance
            // Queue component setup/maybe clearing?
            // Startup live ingest process(es)
        }

        private void SetupError(string msg)
        {
            Console.Error.WriteLine($"Fatal error during setup: {msg}");
            Assert.Fail();
        }
    }
}