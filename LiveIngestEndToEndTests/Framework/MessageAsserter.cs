using System;
using ICAT4IngestLibrary.ActiveMQ;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Framework
{
    /// <summary>
    /// Methods to assert messages reach the expected queues.
    ///
    /// Most commonly, one of these assertions should be the first in a test
    /// because they poll the expected queue, which accounts for the async
    /// nature of the ingest apps.
    /// </summary>
    public class MessageAsserter
    {
        private readonly AMQClient _amqClient =
            new(Environment.GetEnvironmentVariable("AMQ_BROKER_URL"), true);

        public void ReachesSuccess(
            string filename,
            int timeout = 15,
            int pollingIntervalMs = 3000)
        {
            // Note: NUnit has a bug that means the value delegate is called
            // twice on each poll interval, so don't do anything which relies
            // on only happening once https://github.com/nunit/nunit/issues/2841
            var constraint = Is.True
                .After(timeout).Seconds
                .PollEvery(pollingIntervalMs).MilliSeconds;
            var failMsg = $"{filename} failed to reach success queue within " +
                $"{timeout} seconds";
            Assert.That(() => InSuccessQueue(filename), constraint, failMsg);
        }

        private bool InSuccessQueue(string fileName)
        {
            return _amqClient.IsInQueue(fileName, MessageQueue.ICATSuccess);
        }
    }
}
