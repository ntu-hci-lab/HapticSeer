using System;
using System.Collections.Generic;
using System.Text;
using RedisEndpoint;
namespace EventDetectors
{
    class InertiaDetector
    {
        private Subscriber speedSubscriber, inputSubscriber;
        private LinkedList<ushort> speed = new LinkedList<ushort>(); // 1/5 Sec
        private double xInput = 0d, accelX = 0d, accelY = 0d;

        public InertiaDetector(string url, ushort port)
        {
            for (int i = 0; i < 30; i++) speed.AddLast(0);

            speed.AddLast(0);
            speedSubscriber = new Subscriber(url, port);
            inputSubscriber = new Subscriber(url, port);
            speedSubscriber.SubscribeTo("SPEED");
            inputSubscriber.SubscribeTo("XINPUT");

            speedSubscriber.msgQueue.OnMessage(msg => InertiaFunctions.Parser(msg.Message, speed));
            inputSubscriber.msgQueue.OnMessage(msg => InertiaFunctions.Parser(msg.Message, speed));
        }
        
    }
}
