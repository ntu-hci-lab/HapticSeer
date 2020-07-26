using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;
namespace EventDetectors
{
    class FiringDetector
    {
        private Subscriber bulletSubscriber, inputSubscriber;
        private StateObject state = new StateObject();

        public FiringDetector(string url, ushort port)
        {

            bulletSubscriber = new Subscriber(url, port);
            inputSubscriber = new Subscriber(url, port);
            bulletSubscriber.SubscribeTo("BULLET");
            inputSubscriber.SubscribeTo("XINPUT");

            bulletSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
            inputSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
        }

    }
}
