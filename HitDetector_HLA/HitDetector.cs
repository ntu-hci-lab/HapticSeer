using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace EventDetectors
{
    class HitDetector
    {
        private Subscriber bloodSubscriber, hitSubscriber;
        private Publisher commonPublisher;
        private StateObject state;

        public HitDetector(string url, ushort port,
            string bloodInlet, 
            string hitOutlet = null, Publisher publisher = null)
        {
            if (publisher == null) commonPublisher = new Publisher(url, port);
            else commonPublisher = publisher;

            state = new StateObject(commonPublisher);
            state.bloodInlet = bloodInlet;
            state.hitOutlet = hitOutlet;
            bloodSubscriber = new Subscriber(url, port);
            bloodSubscriber.SubscribeTo(bloodInlet);
            bloodSubscriber.msgQueue.OnMessage(msg => HitFunctions.Router(msg.Channel, msg.Message, ref state));
        }

    }
}
