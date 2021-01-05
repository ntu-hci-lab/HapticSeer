
using System;
using RedisEndpoint;
namespace BF1Detectors
{
    class Program
    {
        const string URL = "localhost";
        const ushort PORT = 6380;

        static int Main(string[] args)
        {
            FiringDetector f = new FiringDetector(URL,
                PORT,
                enableHighFreqWeapons: true,
                args[0],
                args[1],
                args[2],
                args[3]);
            _ = Console.ReadKey();
            return 0;
        }
    }
}