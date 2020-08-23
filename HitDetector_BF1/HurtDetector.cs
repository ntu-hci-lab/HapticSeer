using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;
namespace BF1Detectors
{
    class HurtDetector
    {
        private Subscriber bloodSubscriber, hitSubscriber;
        private Publisher commonPublisher;
        private StateObject state;

        public HurtDetector(string url, ushort port, string bloodInlet, string hitInlet, string incomingOutlet = null)
        {
            commonPublisher = new Publisher(url, port);

            bloodSubscriber = new Subscriber(url, port);
            hitSubscriber = new Subscriber(url, port);

            state = new StateObject(commonPublisher);
            state.BloodInlet = bloodInlet;
            state.HitInlet = hitInlet;
            state.IncomingOutlet = incomingOutlet;

            bloodSubscriber.SubscribeTo(bloodInlet);
            hitSubscriber.SubscribeTo(hitInlet);

            bloodSubscriber.msgQueue.OnMessage(msg => HurtFunctions.Router(msg.Channel, msg.Message, ref state));
            hitSubscriber.msgQueue.OnMessage(msg => HurtFunctions.Router(msg.Channel, msg.Message, ref state));
        }

    }
}
