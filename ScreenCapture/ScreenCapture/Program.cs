using ImageProcessModule;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ScreenCapture
{
    class Program
    {
        /// <summary>
        /// Specify the scenario that the program runs
        /// </summary>
        public enum GameType
        {
            None,
            HLA,
            PC2,
            BF1,
            GR
        }
        public static Stopwatch globalStopwatch;
        public static StreamWriter[] logWriters;
        public static long startTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        static BitmapBuffer bitmapBuffer = new BitmapBuffer();
        static CaptureMethod captureMethod;

        /// <summary>
        /// Parse Argument to get the Capture Method
        /// </summary>
        /// <param name="args">Args from Main</param>
        /*static void ArgumentParser(string[] args)
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
        }*/
        static void Main(string[] args)
        {
            globalStopwatch = new Stopwatch();
            globalStopwatch.Start();

            //Thread.Sleep(5000);
            Console.CancelKeyPress +=
                new ConsoleCancelEventHandler((o, t) =>
                {
                    captureMethod.Stop();   //Stop Capturing
                });

            captureMethod = new LocalCapture(bitmapBuffer);

            // Check the Capture Method from args
            //if (args.Length == 0)
            //    captureMethod = new LocalCapture(bitmapBuffer); // Default: Local Capture
            //else
            //    ArgumentParser(args); // Parse Arguments

            //if (captureMethod == null)
            //    throw new Exception("Error! CaptureMethod is null!");

            // Start Capture
            captureMethod.Start();

            // Start dispatch frames
            bitmapBuffer.StartDispatchToImageProcessBase();

            /* FeatureExtractors arrivalEvents = FeatureExtractors.InitFeatureExtractor(
                (int)Enum.Parse(typeof(GameType), args[0]),
                args.Skip(1).ToArray()
            );*/

            FeatureExtractors arrivalEvents = FeatureExtractors.InitFeatureExtractor(
                (int)Enum.Parse(typeof(GameType), "GR"),
                new string[1]
            );

            // Do Cache Optimizer
            CacheOptimizer.Init();
            CacheOptimizer.ResetAllAffinity();
        }
    }

}
