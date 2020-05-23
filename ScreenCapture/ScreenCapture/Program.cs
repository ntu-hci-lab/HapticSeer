using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    class Program
    {
        static BitmapBuffer bitmapBuffer = new BitmapBuffer();
        static CaptureMethod captureMethod;
        static void ArgumentParser(string[] args)
        {
            if (String.Compare(args[0], "Local", StringComparison.OrdinalIgnoreCase) == 0)
                captureMethod = new LocalCapture(bitmapBuffer); //Default: Local Capture
            else if (String.Compare(args[0], "CaptureCard", StringComparison.OrdinalIgnoreCase) == 0)
                captureMethod = new CardCapture(bitmapBuffer);  //Fetch Image From Capture Card
            else
            {
                Console.WriteLine();
                Console.WriteLine("Wrong Argument!");
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{Process.GetCurrentProcess().ProcessName} [ImageSource]");
                Console.WriteLine($"\t[ImageSource]: Local or CaptureCard");
                Console.WriteLine();
            }
        }
        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            CacheOptimizer.Init();
            CacheOptimizer.ResetAllAffinity();
            if (args.Length == 0)
                captureMethod = new LocalCapture(bitmapBuffer); //Default: Local Capture
            else
                ArgumentParser(args);
            if (captureMethod == null)
                return;
            captureMethod.Start();
            CacheOptimizer.ResetAllAffinity();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            captureMethod?.Stop();
        }
    }
}
