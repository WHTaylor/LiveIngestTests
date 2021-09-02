﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using LiveIngestEndToEndTests.Framework;
using NUnit.Framework;
using ICAT4IngestLibrary.ActiveMQ;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]
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
            try
            {
                TestDataVerifier.VerifyTestData();
            }
            catch (Exception e)
            {
                SetupError(e.Message);
            }
            ICAT.ConnectClient();
            TempDataArchive.Create();

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

            // Setup ICAT data? AKA delete leftovers in advance

            TestContext.Progress.WriteLine("Clearing test queues");
            var amqClient = new AMQClient(
                "tcp://icatdevingest.isis.cclrc.ac.uk:61616", true);
            amqClient.DeleteTestQueues();

            try
            {
                Task.WaitAll(
                    Task.Run(() => _processes[Application.FileWatcher]
                        .Start(TempDataArchive.RootDir)),
                    Task.Run(() => _processes[Application.LiveMonitor].Start()),
                    Task.Run(() => _processes[Application.XMLtoICAT].Start()));
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
            TestContext.Error.WriteLine($"Fatal error during setup: {msg}");
            Assert.Fail();
        }
    }
}