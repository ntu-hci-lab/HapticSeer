using System;
using RedisEndpoint;
namespace BF1Detectors
{
    class Program
    {
        const string URL = "localhost";
        const ushort PORT = 6380;


        private static Publisher commonPublisher = new Publisher(URL, PORT);
        static int Main(string[] args)
        {
            HurtDetector h = new HurtDetector(URL, PORT, args[0], args[1], args[2]);
            _ = Console.ReadKey();
            return 0;
        }
    }
}