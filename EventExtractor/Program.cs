using System;
using System.Collections.Generic;
using System.Text;

namespace EventDetectors
{
    class Program
    {
        static int Main()
        {
            FiringDetector e = new FiringDetector("localhost", 6380);
            _ = Console.ReadKey();
            return 0;
        }
    }
}