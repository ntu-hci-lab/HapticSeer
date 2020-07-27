using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;
namespace EventDetectors
{
    class FiringDetector
    {
        private Subscriber bulletSubscriber, inputSubscriber;
        private Publisher commonPublisher;
        private StateObject state;

        public FiringDetector(string url, ushort port)
        {
            commonPublisher = new Publisher(url, port);
            bulletSubscriber = new Subscriber(url, port);
            inputSubscriber = new Subscriber(url, port);

            state = new StateObject(commonPublisher);
            bulletSubscriber.SubscribeTo("BULLET");
            inputSubscriber.SubscribeTo("XINPUT");

            bulletSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
            inputSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
        }

    }
}
