using RedisEndpoint;
namespace PC2Detectors
{
    class InertiaDetector
    {
        private Subscriber speedSubscriber, inputSubscriber;
        private Publisher commonPublisher;
        private StateObject state;

        /// <summary>
        /// Initialize an inertia detector
        /// </summary>
        /// <param name = "url" > URL of Redis server</param>
        /// <param name = "port" > Port number of Redis server</param>
        /// <param name = "ioChannels" > Channel names of IO: 0->speedInlet, 1->accYOutlet, 2->xInputInlet (Optional), 3->accXOutlet (Optional)</param>
        public InertiaDetector(string url, ushort port, params string[] ioChannels)
        {
            commonPublisher = new Publisher(url, port);
            state = new StateObject(commonPublisher);

            if (ioChannels.Length>=2)
            {
                speedSubscriber = new Subscriber(url, port);
                state.speedInlet = ioChannels[0];
                state.accYOutlet = ioChannels[1];
                speedSubscriber.SubscribeTo(ioChannels[0]);
                // Register the router as a callback to the message queue. 
                speedSubscriber.msgQueue.OnMessage(msg => InertiaFunctions.Router(msg.Channel, msg.Message, ref state));
            }
            if (ioChannels.Length == 4)
            {
                inputSubscriber = new Subscriber(url, port);
                state.xinputInlet = ioChannels[2];
                state.accXOutlet = ioChannels[3];
                inputSubscriber.SubscribeTo(ioChannels[2]);
                inputSubscriber.msgQueue.OnMessage(msg => InertiaFunctions.Router(msg.Channel, msg.Message, ref state));
            }
        }

    }
}
