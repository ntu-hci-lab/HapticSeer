
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using RedisEndpoint;
using LatencyLogger;
namespace HLADetectors
{
    class Program
    {
        const string URL = "localhost";
        const ushort PORT = 6380;

        public static LatencyLoggerBase loggers;

        static int Main(string[] args)
        {
            loggers = new LatencyLoggerBase(
                new string[] { "firing_detector" }, "HLA"
            );
            FiringDetector f = new FiringDetector(URL, PORT, enableHighFreqWeapons: false, args[0], args[1], args[2]);
            _ = Console.ReadKey();
            return 0;
        }
    }
}