using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    class Program
    {
        static void ArgumentParser()
        {
            Console.WriteLine("");
        }
        static void Main(string[] args)
        {
            //if (args.Length > 0)
            CaptureMethod t = new CardCapture(new BitmapBuffer());
            t.Start();
        }
    }
}
