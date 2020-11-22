
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using ImageProcessModule.ProcessingClass;
using Tesseract;

using System.Globalization;
using static ImageProcessModule.ImageProcessBase;
using System.Diagnostics;

namespace ScreenCapture
{


    class PC2 : FeatureExtractors
    {

#if LOG
        static DateTime startTime = DateTime.Now;
        static StreamWriter csvWriter = new StreamWriter($"{startTime.ToString("mm_ss_ffff")}_send.csv");
        static long msgCnt = 0;
#endif

        private SpeedImageProcess speedImageProcess = new SpeedImageProcess();
        private int speed = 0; // current speed
        private int preSpeed = 0; // previous speed
        private string speedOutlet;

#if DEBUG
        private StreamWriter streamWriter;
        private DateTime localDate;
#endif

        public PC2(string speedOutlet = null) : base()
        {
#if DEBUG
            this.speedOutlet = speedOutlet;
            streamWriter = new StreamWriter("detected.txt");
#endif

            ImageProcessesList.Add(new ImageProcess(1541 / 1720f, 1601 / 1720f, 962d / 1080, 1002d / 1080, ImageScaleType.OriginalSize, 60));
            ImageProcessesList.Last().NewFrameArrivedEvent += SpeedDetectionEvent;
        }

        private void SpeedDetectionEvent(ImageProcess sender, Mat mat)
        {
#if DEBUG
            localDate = DateTime.Now;
#endif
            var startTick = Program.globalStopwatch.ElapsedTicks;
            /* declare variables for Tesseract */
            Pix pixImage;
            Page page;

            try
            {
                Bitmap BitmapFrame = mat.ToBitmap();
                /* image processing */
                speedImageProcess.ToBlackWhite(BitmapFrame); // grayscale(black and white)
                // BitmapFrame = speedImageProcess.NegativePicture(BitmapFrame); //turn into negative image
                speedImageProcess.ResizeImage(BitmapFrame, 120, 76); // enlarge image(x2)
                pixImage = PixConverter.ToPix(BitmapFrame); // PixConverter is unable to work at Tesseract 3.3.0
                page = tesseractEngine.Process(pixImage, PageSegMode.SingleBlock);
                string speedStr = page.GetText(); // Recognized result
                page.Dispose();
                pixImage.Dispose();

                ///* Parse str to int */
                bool isParsable = Int32.TryParse(speedStr, out speed);
                if (!isParsable)
                {
                    // Console.WriteLine("Speed could not be parsed.");
                    speed = preSpeed; // Can't detect speed, use the previous speed value
                }

                /* Prevent negative number or large number */
                if (speed < 0 || speed > 300)
                    speed = preSpeed;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error message: " + ex.Message);
            }
#if LOG
            csvWriter.WriteLine($"{speed},{++msgCnt},{(DateTime.Now - startTime).TotalMilliseconds}");
            if(speedOutlet!=null)
                publisher.Publish(speedOutlet, $"{speed},{msgCnt}");
#endif
#if !LOG
            if (speedOutlet != null) 
            {
                publisher.Publish(speedOutlet, $"{speed}");
                Program.logWriters[0].WriteLineAsync(
$"{(double)startTick / Stopwatch.Frequency * 1000},{(double)Program.globalStopwatch.ElapsedTicks / Stopwatch.Frequency * 1000}");
            }
                
#endif

#if DEBUG
            var time = localDate.Minute * 60 * 1000 + localDate.Second * 1000 + localDate.Millisecond;
            Console.WriteLine("  -Smoothed speed: " + speed + " kmh\n");
            streamWriter.WriteLine(time+","+speed);
#endif
        }
    }
}
