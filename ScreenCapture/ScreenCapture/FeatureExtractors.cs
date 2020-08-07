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

        /// Initialize Tesseract object
        /// Remember to add tessdata directory
        protected static TesseractEngine ocr = new TesseractEngine(Path.GetFullPath(@"..\..\"), "KomuB", EngineMode.Default);
        protected static TesseractEngine ocr_eng = new TesseractEngine(Path.GetFullPath(@"..\..\"), "Alyx+eng", EngineMode.Default);
        protected static Publisher publisher = new Publisher("localhost", 6380);
        protected List<ImageProcess> ImageProcessesList = new List<ImageProcess>();
        protected FeatureExtractors() {}

        public static FeatureExtractors InitFeatureExtractor(int gameID)
        {
            switch (gameID)
            {
                case 1:
                    return new HLA();
                case 2:
                    return new PC2();
                case 3:
                    return new BF1();
                default:
                    throw new NotImplementedException("Invalid gameID");
            }
        }
    }
}
