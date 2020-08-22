using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace EventDetectors
{
    class Program
    {
#if DEBUG
        public static Stopwatch globalSW = new Stopwatch();
#endif
        static int Main(string[] args)
        {
            foreach (string s in args) Console.Write($"{s} ");
            Console.WriteLine(args);
            Thread.Sleep(5000);
            globalSW.Start();
            InertiaDetector inertiaDetector = new InertiaDetector("localhost", 6380, args[0], args[1], args[2], args[3]);
            _ = Console.ReadKey();
            return 0;
        }
    }
}