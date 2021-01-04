
using System;
using RedisEndpoint;
using EvaluationLogger;
namespace BF1Detectors
{
    class Program
    {
        const string URL = "localhost";
        const ushort PORT = 6380;

        public static Base loggers;

        static int Main(string[] args)
        {
            loggers = new Base(
            new string[] { "firing_detector" }, "BF1"
            );
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