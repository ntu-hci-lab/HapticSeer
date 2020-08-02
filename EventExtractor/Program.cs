using System;
using RedisEndpoint;

namespace EventDetectors
{
    class Program
    {
        const string URL = "localhost";
        const ushort PORT = 6380;
        private static Publisher commomPublisher = new Publisher(URL, PORT);
        static int Main()
        {

            FiringDetector f = new FiringDetector(URL, PORT, enableAutoWeapons: true, commomPublisher);
            //HurtDetector h = new HurtDetector(URL, PORT, commomPublisher);
            _ = Console.ReadKey();
            return 0;
        }
    }
}