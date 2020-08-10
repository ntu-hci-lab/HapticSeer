using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;
namespace EventDetectors
{
    class HurtDetector
    {
        private Subscriber bloodSubscriber, hitSubscriber;
        private Publisher commonPublisher;
        private HealthState state;

        public HurtDetector(string url, ushort port, Publisher publisher = null)
        {
            if (publisher == null) commonPublisher = new Publisher(url, port);
            else commonPublisher = publisher;

            state = new HealthState(commonPublisher);
            bloodSubscriber = new Subscriber(url, port);
            bloodSubscriber.SubscribeTo("BLOOD");
            bloodSubscriber.msgQueue.OnMessage(msg => HurtFunctions.Router(msg.Channel, msg.Message, ref state));
        }

    }
}
