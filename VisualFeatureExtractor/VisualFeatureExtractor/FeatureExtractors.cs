using ImageProcessModule.ProcessingClass;
using System;
using System.Collections.Generic;
using System.IO;
using Tesseract;
using RedisEndpoint;

namespace VisualFeatureExtractor
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

        /// <summary>
        /// The function acts as a "factory" in the factory pattern
        /// </summary>
        /// <param name="gameID">The game ID for selecting which extractor to instantiate</param>
        /// <param name="outlets">Some extractors will send their data to upstream extractors/detectors. This the an array containing theire names.</param>
        /// <returns>A feature extractor instance</returns>
        public static FeatureExtractors InitFeatureExtractor(int gameID, string[] outlets)
        {
            switch (gameID)
            {
                // Example case: we use a pretrained Tesseract model to get the speedometer in Project CARS 2
                case 0:
                    tesseractEngine = new TesseractEngine(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), "KomuB", EngineMode.Default);
                    return new PC2(outlets[0]);
                default:
                    throw new NotImplementedException("Invalid gameID");
            }
        }
    }
}