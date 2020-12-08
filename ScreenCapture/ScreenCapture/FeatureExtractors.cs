using ImageProcessModule.ProcessingClass;
using System;
using System.Collections.Generic;
using System.IO;
using Tesseract;
using RedisEndpoint;

namespace ScreenCapture
{
    public class FeatureExtractors
    {
        public readonly static string SolutionRoot = Path.GetFullPath(Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));

        /// Initialize Tesseract object
        /// Remember to add tessdata directory
        protected static TesseractEngine tesseractEngine;
        protected static Publisher publisher = new Publisher("localhost", 6380);
        protected List<ImageProcess> ImageProcessesList = new List<ImageProcess>();
        protected FeatureExtractors() { }

        public static FeatureExtractors InitFeatureExtractor(int gameID, string[] outlets)
        {
            switch (gameID)
            {
                case 1:
                    Program.logWriters = new StreamWriter[2];
                    Program.logWriters[0] = new StreamWriter(Path.Combine(SolutionRoot,
                $"blood_extractor_HLA_{Program.startTimeStamp}.csv"))
                    {
                        AutoFlush = true
                    };
                    Program.logWriters[1] = new StreamWriter(Path.Combine(SolutionRoot,
                $"bullet_extractor_HLA_{Program.startTimeStamp}.csv"))
                    {
                        AutoFlush = true
                    };
                    tesseractEngine = new TesseractEngine(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "HalfLife", EngineMode.Default);
                    return new HLA(outlets[0], outlets[1]);
                case 2:
                    Program.logWriters = new StreamWriter[1];
                    Program.logWriters[0] = new StreamWriter(Path.Combine(SolutionRoot,
                $"speed_extractor_PC2_{Program.startTimeStamp}.csv"))
                    {
                        AutoFlush = true
                    };
                    tesseractEngine = new TesseractEngine(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "KomuB", EngineMode.Default);
                    return new PC2(outlets[0]);
                case 3:
                    Program.logWriters = new StreamWriter[3];
                    Program.logWriters[0] = new StreamWriter(Path.Combine(SolutionRoot,
                $"indicator_extractor_BF1_{Program.startTimeStamp}.csv"))
                    {
                        AutoFlush = true
                    };
                    Program.logWriters[1] = new StreamWriter(Path.Combine(SolutionRoot,
                $"blood_extractor_BF1_{Program.startTimeStamp}.csv"))
                    {
                        AutoFlush = true
                    };
                    Program.logWriters[2] = new StreamWriter(Path.Combine(SolutionRoot,
                $"bullet_extractor_BF1_{Program.startTimeStamp}.csv"))
                    {
                        AutoFlush = true
                    };
                    tesseractEngine = new TesseractEngine(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "KomuB", EngineMode.Default);
                    return new BF1(outlets[0], outlets[1], outlets[2]);
                case 4:
                    return new GR();
                default:
                    throw new NotImplementedException("Invalid gameID");
            }
        }
    }
}