using ImageProcessModule;
using ImageProcessModule.ProcessingClass;
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
        /// <summary>
        /// Parse Argument to get the Capture Method
        /// </summary>
        /// <param name="args"></param>
        static void ArgumentParser(string[] args)
        {
            if (String.Compare(args[0], "Local", StringComparison.OrdinalIgnoreCase) == 0)
                captureMethod = new LocalCapture(bitmapBuffer); //Default: Local Capture
            else if (String.Compare(args[0], "CaptureCard", StringComparison.OrdinalIgnoreCase) == 0)
                captureMethod = new CardCapture(bitmapBuffer);  //Fetch Image From Capture Card
            else
            {
                // Unknown Argument
                Console.WriteLine();
                Console.WriteLine("Wrong Argument!");
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{Process.GetCurrentProcess().ProcessName} [ImageSource]");
                Console.WriteLine($"\t[ImageSource]: Local or CaptureCard");
                Console.WriteLine();
                // Force close the process
                Process.GetCurrentProcess().Kill();
            }
        }
        static void Main(string[] args)
        {
            Console.CancelKeyPress +=
                new ConsoleCancelEventHandler((o, t) =>
                {
                    captureMethod.Stop();   //Stop Capturing
                });

            // Check the Capture Method from args
            if (args.Length == 0)
                captureMethod = new LocalCapture(bitmapBuffer); // Default: Local Capture
            else
                ArgumentParser(args); // Parse Arguments

            if (captureMethod == null)
                throw new Exception("Error! CaptureMethod is null!");

            // Start Capture
            captureMethod.Start();

            // Start dispatch frames
            bitmapBuffer.StartDispatchToImageProcessBase();

            // Do Cache Optimizer
            CacheOptimizer.Init();
            CacheOptimizer.ResetAllAffinity();
            ImageProcess imageProcess = new ImageProcess(0.5);
            imageProcess.NewFrameArrivedEvent += new ImageProcess.NewFrameArrived((mat) =>
            {
                mat.Save("O:\\p.png");
            });
        }

    }
}
