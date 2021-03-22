using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
namespace PC2Detectors
{
    class Program
    {
        static int Main(params string[] args)
        {
            Thread.Sleep(5000);

            // If an inlet for speedometer and an outlet for the longitudinal acceleration was provided 
            if (args.Length == 2)
            {
                InertiaDetector inertiaDetector = new InertiaDetector("localhost", 6380, args[0], args[1]);
            }
            // If an inlet for controller input and an outlet for the lateral acceleration was also provided 
            else if (args.Length == 4)
            {
                InertiaDetector inertiaDetector = new InertiaDetector("localhost", 6380, args[0], args[2], args[1], args[3]);
            }
            else
            {
                throw new ArgumentException("Wrong number of arguments provided!");
            }
            
            _ = Console.ReadKey();
            return 0;
        }
    }
}