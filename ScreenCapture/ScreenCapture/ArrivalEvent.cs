using ImageProcessModule.ProcessingClass;
using System;
using System.Collections.Generic;
using System.IO;
using Tesseract;
using RedisEndpoint;

namespace ScreenCapture
{
    public class ArrivalEvent
    {

        /// Initialize Tesseract object
        /// Remember to add tessdata directory
        protected static TesseractEngine ocr = new TesseractEngine(Path.GetFullPath(@"..\..\"), "eng", EngineMode.Default);
        protected static Publisher publisher = new Publisher("localhost", 6380);
        protected List<ImageProcess> ImageProcessesList = new List<ImageProcess>();
        protected ArrivalEvent() {}

        public static ArrivalEvent SetArrivalEventsByID(int gameID)
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
