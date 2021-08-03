using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace LiveIngestEndToEndTests
{
    [SetUpFixture]
    public class Setup
    {
        private readonly Dictionary<Application, IngestProcess>
            _processes = new();

        [OneTimeSetUp]
        public void EnvironmentSetup()
        {
            foreach (var app in (Application[]) Enum.GetValues(
                typeof(Application)))
            {
                try
                {
                    _processes[app] = new IngestProcess(app);
                }
                catch (FileNotFoundException e)
                {
                    SetupError(e.Message);
                }
            }

            var toWatch = new DataArchiverCreator().CreateDataArchive();

            // Setup ICAT data? AKA delete leftovers in advance
            // Queue component setup/maybe clearing?

            try
            {
                _processes[Application.FileWatcher].Start(toWatch);
                _processes[Application.LiveMonitor].Start();
                _processes[Application.XMLtoICAT].Start();
            }
            catch (ApplicationException e)
            {
                SetupError(e.Message);
            }

            TestContext.Progress.WriteLine("All started");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            foreach (var proc in _processes.Values)
            {
                proc.Stop();
            }
        }

        private void SetupError(string msg)
        {
            TestContext.Progress.WriteLine($"Fatal error during setup: {msg}");
            Assert.Fail();
        }
    }
}