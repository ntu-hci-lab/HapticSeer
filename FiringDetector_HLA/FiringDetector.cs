using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;
namespace HLADetectors
{
    class FiringDetector
    {
        private Subscriber bulletSubscriber, inputSubscriber, impulseSubscriber;
        private Publisher commonPublisher;
        private StateObject state;

        public FiringDetector(string url, ushort port, bool enableHighFreqWeapons, 
            string bulletInlet, string openvrInlet,
            string fireOutlet=null)
        {
            commonPublisher = new Publisher(url, port);


            state = new StateObject(commonPublisher);
            state.bulletInlet = bulletInlet;
            state.openvrInlet = openvrInlet;
            state.fireOutlet = fireOutlet;

            bulletSubscriber = new Subscriber(url, port);
            inputSubscriber = new Subscriber(url, port);

            bulletSubscriber.SubscribeTo(bulletInlet);
            inputSubscriber.SubscribeTo(openvrInlet);

            /*if (enableHighFreqWeapons)
            {
                impulseSubscriber = new Subscriber(url, port);
                impulseSubscriber.SubscribeTo("IMPULSE");
                impulseSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
            }*/

            bulletSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
            inputSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
        }
    }
}
