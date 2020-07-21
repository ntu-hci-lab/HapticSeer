using System;
using System.Collections.Generic;
using System.Text;

namespace EventExtractors
{
    class Program
    {
        static int Main()
        {
            InertiaDetector e = new InertiaDetector("localhost", 6380);
            _ = Console.ReadKey();
            return 0;
        }
    }
}