using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;

namespace BF1Detectors
{
    class FiringDetector
    {
        private Subscriber bulletSubscriber, inputSubscriber, pulseSubscriber;
        private Publisher commonPublisher;
        private StateObject state;

        public FiringDetector(string url, ushort port, bool enableHighFreqWeapons,
            string bulletInlet, string xinputInlet, string pulseInlet=null,
            string fireOutlet = null)
        {

            commonPublisher = new Publisher(url, port);

            state = new StateObject(commonPublisher);

            state.BulletInlet = bulletInlet;
            state.XinputInlet = xinputInlet;
            state.FireOutlet = fireOutlet;
            bulletSubscriber = new Subscriber(url, port);
            inputSubscriber = new Subscriber(url, port);

            bulletSubscriber.SubscribeTo(bulletInlet);
            inputSubscriber.SubscribeTo(xinputInlet);

            if (enableHighFreqWeapons)
            {
                if (pulseInlet == null) throw new MissingMemberException(
                    "Error: Pulse Inlet must not be missing while enable high freq. weapons");
                state.PulseInlet = pulseInlet;
                pulseSubscriber = new Subscriber(url, port);
                pulseSubscriber.SubscribeTo(pulseInlet);
                pulseSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
            }

            bulletSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
            inputSubscriber.msgQueue.OnMessage(msg => FiringFunctions.Router(msg.Channel, msg.Message, ref state));
        }
    }
}
