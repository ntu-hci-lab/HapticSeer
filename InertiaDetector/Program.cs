using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
namespace PC2Detectors
{
    class Program
    {
        static int Main(string[] args)
        {
            Thread.Sleep(5000);
            InertiaDetector inertiaDetector = new InertiaDetector("localhost", 6380, args[0], args[1], args[2], args[3]);
            _ = Console.ReadKey();
            return 0;
        }
    }
}