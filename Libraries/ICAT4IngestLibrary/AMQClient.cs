using System;
using System.Collections;
using System.Collections.Generic;
using log4net;
using Apache.NMS;
using Apache.NMS.Util;

namespace ICAT4IngestLibrary.ActiveMQ
{
    public enum MessageQueue
    {
        Initial, Long,
        ICAT, ICATLong, ICATSuccess,
        PreICATError, PostICATError,
        DMF, DMFLong, DMFError, DMFSuccess
    }

    public class AMQClient
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly bool testMode;
        private readonly ISession session;
        private readonly Dictionary<MessageQueue, IMessageProducer> producers = new Dictionary<MessageQueue, IMessageProducer>();
        private readonly Dictionary<MessageQueue, IMessageConsumer> consumers = new Dictionary<MessageQueue, IMessageConsumer>();

        private readonly Dictionary<MessageQueue, string> queueNames = new Dictionary<MessageQueue, string>
        {
            { MessageQueue.Initial, "Initial ICAT" },
            { MessageQueue.Long, "Long" },
            { MessageQueue.ICAT, "ICAT" },
            { MessageQueue.ICATLong, "Long ICAT" },
            { MessageQueue.ICATSuccess, "ICAT Success" },
            { MessageQueue.PreICATError, "pre ICAT Error Queue" },
            { MessageQueue.PostICATError, "post ICAT Error Queue" },
            { MessageQueue.DMF, "DMF" },
            { MessageQueue.DMFLong, "DMF Long" },
            { MessageQueue.DMFError, "DMF Error" },
            { MessageQueue.DMFSuccess, "DMF Success" },
        };

        public AMQClient(string brokerUrl, bool testMode=false)
        {
            var factory = new Apache.NMS.ActiveMQ.ConnectionFactory(brokerUrl);
            var connection = factory.CreateConnection();
            session = connection.CreateSession();
            connection.Start();
            this.testMode = testMode;
        }

        public AMQClient ConsumingFrom(params MessageQueue[] queues)
        {
            foreach (var queue in queues)
            {
                /* Setting prefetchSize=0 causes the application to poll the queue, rather
                 * than messages being pushed from the queue and cached application side.
                 *
                 * This makes sense for our apps because processing messages is slow relative
                 * to retrieving them, so there's little downside, and polling reduces the
                 * chance of losing messages - if an app crashes whilst processing a
                 * message, any in the cache have already left the queue so will be lost.
                 */
                var dest = session.GetDestination($"queue://{QueueName(queue)}?consumer.prefetchSize=0");
                consumers[queue] = session.CreateConsumer(dest);
            }
            return this;
        }

        public AMQClient ProducingTo(params MessageQueue[] queues)
        {
            foreach (var queue in queues)
            {
                var dest = session.GetDestination($"queue://{QueueName(queue)}");
                producers[queue] = session.CreateProducer(dest);
            }
            return this;
        }

        public IMessage ReceiveFrom(MessageQueue queue)
        {
            if (consumers.TryGetValue(queue, out var c))
            {
                return c.Receive();
            }
            else
            {
                throw new Exception($"Cannot read from '{queue}' before connecting to it " +
                    $"using AMQClient::ConsumingFrom");
            }
        }

        public void SendTo(MessageQueue queue, string message)
        {
            if (producers.TryGetValue(queue, out var p))
            {
                p.Send(session.CreateTextMessage(message));
            }
            else
            {
                throw new Exception($"Cannot send to '{queue}' before connecting to it " +
                    $"using AMQClient::ProducingTo");
            }
        }

        public IEnumerator GetMessages(MessageQueue queue)
        {
            IDestination requestDestination = SessionUtil.GetDestination(session, QueueName(queue));
            IQueueBrowser queueBrowser = session.CreateBrowser((IQueue)requestDestination);
            IEnumerator messages = queueBrowser.GetEnumerator();
            return messages;
        }

        public bool IsInQueue(string message, MessageQueue queue)
        {
            var dest = session.GetQueue(QueueName(queue));
            var browser = session.CreateBrowser(dest);
            foreach(ITextMessage m in browser)
            {
                if (m.Text.Contains(message)) return true;
            }
            return false;
        }
 
        private string QueueName(MessageQueue queue)
        {
            var prefix = testMode ? "(Test) " : "";
            return $"{prefix}{queueNames[queue]}";
        }

        public void DeleteTestQueues()
        {
            if (!testMode)
            {
                log.Warn("DeleteTestQueues called whilst not in test mode, not doing anything");
                return;
            }
            var queues = (MessageQueue[])Enum.GetValues(typeof(MessageQueue));
            foreach (var queue in queues)
            {
                var dest = session.GetQueue(QueueName(queue));
                session.DeleteDestination(dest);
            }
        }
    }
}
