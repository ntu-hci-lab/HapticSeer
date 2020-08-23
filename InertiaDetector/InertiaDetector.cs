using RedisEndpoint;
namespace EventDetectors
{
    class InertiaDetector
    {
        private Subscriber speedSubscriber, inputSubscriber;
        private Publisher commonPublisher;
        private StateObject state;

        public InertiaDetector(string url, ushort port, 
            string speedInlet, string xinputInlet, 
            string accXOutlet, string accYOutlet)
        {
            commonPublisher = new Publisher(url, port);
            speedSubscriber = new Subscriber(url, port);
            inputSubscriber = new Subscriber(url, port);

            state = new StateObject(commonPublisher);
            state.speedInlet = speedInlet;
            state.xinputInlet = xinputInlet;
            state.accXOutlet = accXOutlet;
            state.accYOutlet = accYOutlet;
            speedSubscriber.SubscribeTo(speedInlet);
            inputSubscriber.SubscribeTo(xinputInlet);

            speedSubscriber.msgQueue.OnMessage(msg => InertiaFunctions.Router(msg.Channel, msg.Message, ref state));
            inputSubscriber.msgQueue.OnMessage(msg => InertiaFunctions.Router(msg.Channel, msg.Message, ref state));
        }

    }
}
