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

        private static Publisher commonPublisher = new Publisher(URL, PORT);
        static int Main(string[] args)
        {
            loggers = new Base(
                new string[] { "hit_detector" }, "BF1"
            );
            HurtDetector h = new HurtDetector(URL, PORT, args[0], args[1], args[2]);
            _ = Console.ReadKey();
            return 0;
        }
    }
}