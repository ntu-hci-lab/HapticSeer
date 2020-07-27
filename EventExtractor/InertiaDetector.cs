using RedisEndpoint;
namespace EventDetectors
{
    class InertiaDetector
    {
        private Subscriber speedSubscriber, inputSubscriber;
        private Publisher commonPublisher;
        private StateObject state;

        public InertiaDetector(string url, ushort port)
        {
            commonPublisher = new Publisher(url, port);
            speedSubscriber = new Subscriber(url, port);
            inputSubscriber = new Subscriber(url, port);

            state = new StateObject(commonPublisher);
            speedSubscriber.SubscribeTo("SPEED");
            inputSubscriber.SubscribeTo("XINPUT");

            speedSubscriber.msgQueue.OnMessage(msg => InertiaFunctions.Router(msg.Channel, msg.Message, ref state));
            inputSubscriber.msgQueue.OnMessage(msg => InertiaFunctions.Router(msg.Channel, msg.Message, ref state));
        }

    }
}
