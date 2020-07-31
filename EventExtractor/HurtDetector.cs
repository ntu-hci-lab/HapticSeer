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

            bloodSubscriber = new Subscriber(url, port);
            hitSubscriber = new Subscriber(url, port);

            state = new HealthState(commonPublisher);
            bloodSubscriber.SubscribeTo("BLOOD");
            hitSubscriber.SubscribeTo("HIT");

            bloodSubscriber.msgQueue.OnMessage(msg => HurtFunctions.Router(msg.Channel, msg.Message, ref state));
            hitSubscriber.msgQueue.OnMessage(msg => HurtFunctions.Router(msg.Channel, msg.Message, ref state));
        }

    }
}
