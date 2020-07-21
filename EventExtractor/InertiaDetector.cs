using System;
using System.Collections.Generic;
using System.Text;

namespace EventExtractors
{
    class InertiaDetector
    {
        private PooledSubscriber speedSubscriber, inputSubscriber;
        private ushort curSpeed = 0;
        private double yAxis = 0d;
        public InertiaDetector(string url, ushort port)
        {
            bool flag = false;
            speedSubscriber = new PooledSubscriber(url, port);
            inputSubscriber = new PooledSubscriber(url, port);

            while (!flag)
            {
                speedSubscriber.TrySubscribeTo("SPEED", out flag);
            }
            flag = false;
            while (!flag)
            {
                inputSubscriber.TrySubscribeTo("XINPUT", out flag);
            }

            speedSubscriber.GetMsqInstance().OnMessage((msg) =>
            {
                Console.WriteLine(msg.Message);
            });
            inputSubscriber.GetMsqInstance().OnMessage((msg) =>
            {
                Console.WriteLine(msg.SubscriptionChannel);
            });
        }
    }
}
