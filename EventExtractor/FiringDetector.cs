using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;
namespace EventDetectors
{
    class FiringDetector
    {
        private Subscriber bulletSubscriber, inputSubscriber;
        private byte triggerState = 0;
        private ushort bulletCount;

        public FiringDetector(string url, ushort port)
        {

            bulletSubscriber = new Subscriber(url, port);
            inputSubscriber = new Subscriber(url, port);
            bulletSubscriber.SubscribeTo("BULLET");
            inputSubscriber.SubscribeTo("XINPUT");

            bulletSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref triggerState));
            inputSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref triggerState));
        }

    }
}
