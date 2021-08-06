using System;
using ICAT4IngestLibrary.ActiveMQ;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Framework
{
    public class DelayedAssert
    {
        private readonly AMQClient _amqClient =
            new(Environment.GetEnvironmentVariable("AMQ_BROKER_URL"));

        public void Success(
            string filename,
            int timeout = 10,
            int pollingIntervalMs = 1000)
        {
            var constraint = Is.True
                .After(timeout).Seconds
                .PollEvery(pollingIntervalMs).MilliSeconds;
            Assert.That(() => InSuccessQueue(filename), constraint);
        }

        private bool InSuccessQueue(string fileName)
        {
            TestContext.Progress.WriteLine("Trying...");
            return _amqClient.IsInQueue(fileName, MessageQueue.ICATSuccess);
        }
    }
}
