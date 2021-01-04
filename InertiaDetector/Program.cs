using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using EvaluationLogger;
namespace PC2Detectors
{
    class Program
    {
        public static Base loggers;

        static int Main(string[] args)
        {
            Thread.Sleep(5000);
            loggers = new Base(
                new string[] { "inertia_detector"}, "PC2"
            );
            InertiaDetector inertiaDetector = new InertiaDetector("localhost", 6380, args[0], args[1], args[2], args[3]);
            _ = Console.ReadKey();
            return 0;
        }
    }
}