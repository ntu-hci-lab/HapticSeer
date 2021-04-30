
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
using Accord;

namespace VisualFeatureExtractor
{


    class PC2 : FeatureExtractors
    {
        private const int resetLimit = 3 * 60;
        private SpeedImageProcess speedImageProcess = new SpeedImageProcess();
        private int resetCounter = 0;
        private int speed = 0; // current speed
        private int? preSpeed = null; // previous speed
        private string speedOutlet;

        public PC2(string speedOutlet = null) : base()
        {
            this.speedOutlet = speedOutlet;

            // Instantiate an image processor
            ImageProcessesList.Add(
                new ImageProcess( 
                    1720 / 1920f, 1780 / 1920f, //W
                    940d / 1080, 980d / 1080,   //H
                    ImageScaleType.OriginalSize, 
                    60                                              // Frame per second
                )
            );
            // Apply image processing callback to the image processor
            ImageProcessesList.Last().NewFrameArrivedEvent += SpeedDetectionEvent;
        }

        /// <summary>
        ///  Callback function as image processing logic.
        /// </summary>
        /// <param name="sender">The image processor instance</param>
        /// <param name="mat">An OpenCV matrix for this frame</param>
        private void SpeedDetectionEvent(ImageProcess sender, Mat mat)
        {
            /* declare variables for Tesseract */
            Pix pixImage;
            Page page;

            try
            {
                Bitmap BitmapFrame = mat.ToBitmap();
                /* image processing */
                speedImageProcess.ToBlackWhite(BitmapFrame); // grayscale(black and white)
                speedImageProcess.ResizeImage(BitmapFrame, 120, 80); // enlarge image(x2)
                pixImage = PixConverter.ToPix(BitmapFrame); // PixConverter is unable to work at Tesseract 3.3.0
                page = tesseractEngine.Process(pixImage, PageSegMode.SingleBlock);
                string speedStr = page.GetText(); // Recognized result
                page.Dispose();
                pixImage.Dispose();

                ///* Parse str to int */
                bool isParsable = Int32.TryParse(speedStr, out speed);
                if (preSpeed.HasValue && (!isParsable || speed < 0 || speed > 350 || Math.Abs(preSpeed.Value-speed) > 6)) // 6 = 200m/s^2
                {
                    resetCounter++;
                    Console.WriteLine($"Error: {speed}, {preSpeed}");
                    speed = preSpeed.Value; // Can't detect speed, use the previous speed value
                    if (resetLimit < resetCounter)
                    {
                        resetCounter = 0;
                        preSpeed = null;
                    }
                }
                else if(isParsable)
                {
                    preSpeed = speed;
                    if (speedOutlet != null)
                    {
                        // Send extracted digits by the publisher to a Redis channel named as the value of "speedOulet"
                        Console.WriteLine($"OK: {speed}");
                        publisher.Publish(speedOutlet, $"{speed}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
