using ImageProcessModule;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace VisualFeatureExtractor
{
    class Program
    {
        /// <summary>
        /// Specify the scenario that the program runs
        /// </summary>
        public enum GameType
        {
            PC2,
        }
        public static Stopwatch globalStopwatch;
        public static StreamWriter[] logWriters;
        public static long startTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        static BitmapBuffer bitmapBuffer = new BitmapBuffer();
        static CaptureMethod captureMethod;

        static void Main(string[] args)
        {
            globalStopwatch = new Stopwatch();
            globalStopwatch.Start();

            Thread.Sleep(5000);
            Console.CancelKeyPress +=
                new ConsoleCancelEventHandler((o, t) =>
                {
                    captureMethod.Stop();   //Stop Capturing
                });

            captureMethod = new LocalCapture(bitmapBuffer);

            // Start Capture
            captureMethod.Start();

            // Start dispatch frames
            bitmapBuffer.StartDispatchToImageProcessBase();

            // arg[0]: Game type (String)
            // arg[1..n]: Name of outlet channels (String)
            FeatureExtractors arrivalEvents = FeatureExtractors.InitFeatureExtractor(
               (int)Enum.Parse(typeof(GameType), args[0]),
               args.Skip(1).ToArray()
            );

            // Do Cache Optimizer
            CacheOptimizer.Init();
            CacheOptimizer.ResetAllAffinity();
        }
    }

}
