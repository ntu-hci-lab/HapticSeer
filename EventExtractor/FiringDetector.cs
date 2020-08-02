using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;
namespace EventDetectors
{
    class FiringDetector
    {
        private Subscriber bulletSubscriber, inputSubscriber, impulseSubscriber;
        private Publisher commonPublisher;
        private WeaponState state;

        public FiringDetector(string url, ushort port, bool enableAutoWeapons, Publisher publisher = null)
        {
            if (publisher == null) commonPublisher = new Publisher(url, port);
            else commonPublisher = publisher;

            state = new WeaponState(commonPublisher);

            bulletSubscriber = new Subscriber(url, port);
            inputSubscriber = new Subscriber(url, port);

            bulletSubscriber.SubscribeTo("BULLET");
            inputSubscriber.SubscribeTo("XINPUT");

            if (enableAutoWeapons)
            {
                impulseSubscriber = new Subscriber(url, port);
                impulseSubscriber.SubscribeTo("IMPULSE");
                impulseSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
            }

            bulletSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
            inputSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
        }
    }
}
