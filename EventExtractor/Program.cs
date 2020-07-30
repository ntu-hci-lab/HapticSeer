using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace EventDetectors
{
    class Program
    {
#if DEBUG
        public static Stopwatch globalSW = new Stopwatch();
#endif
        static int Main()
        {
            globalSW.Start();
            InertiaDetector inertiaDetector = new InertiaDetector("localhost", 6380);
            _ = Console.ReadKey();
            return 0;
        }
    }
}