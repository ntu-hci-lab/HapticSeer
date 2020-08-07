using System;
using RedisEndpoint;

namespace EventDetectors
{
    class Program
    {
        const string URL = "localhost";
        const ushort PORT = 6380;
        private static Publisher commonPublisher = new Publisher(URL, PORT);
        static int Main()
        {

            FiringDetector f = new FiringDetector(URL, PORT, enableAutoWeapons: false, commonPublisher);
            HurtDetector h = new HurtDetector(URL, PORT, commonPublisher);
            _ = Console.ReadKey();
            return 0;
        }
    }
}