using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using RedisEndpoint;
using EvaluationLogger;
namespace HLADetectors
{
    class Program
    {
        const string URL = "localhost";
        const ushort PORT = 6380;

        public static Base loggers;

        static int Main(string[] args)
        {
            loggers = new Base(
                new string[] { "hit_detector" }, "HLA"
            );
            HitDetector h = new HitDetector(URL, PORT, args[0], args[1]);
            _ = Console.ReadKey();
            return 0;
        }
    }
}