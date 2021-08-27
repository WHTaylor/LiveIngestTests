using System;
using ICAT4IngestLibrary.ActiveMQ;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Framework
{
    public class DelayedAssert
    {
        private readonly AMQClient _amqClient =
            new(Environment.GetEnvironmentVariable("AMQ_BROKER_URL"),
                true);

        public void Success(
            string filename,
            int timeout = 15,
            int pollingIntervalMs = 3000)
        {
            var constraint = Is.True
                .After(timeout).Seconds
                .PollEvery(pollingIntervalMs).MilliSeconds;
            var failMsg =
                $"{filename} failed to reach success queue within {timeout} seconds";
            Assert.That(() => InSuccessQueue(filename), constraint, failMsg);
        }

        private bool InSuccessQueue(string fileName)
        {
            TestContext.Progress.WriteLine("Trying...");
            return _amqClient.IsInQueue(fileName, MessageQueue.ICATSuccess);
        }
    }
}
