using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ImageProcessModule.ProcessingClass
{
    public class ImageProcess : ImageProcessBase
    {
        /*Initialized By ctor*/
        private double Fraction_Left, Fraction_Right, Fraction_Top, Fraction_Bottom;
        private ImageScaleType IncomingImageScale;
        private int PreferFrameRate;
        /*Initialized By ctor*/

        public delegate void NewFrameArrived(Mat mat);

        // Declare the event.
        public event NewFrameArrived NewFrameArrivedEvent;

        /*For ImageProcessBase Information*/
        protected override double Clipped_Left
        {
            get
            {
                return Fraction_Left;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return Fraction_Top;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return Fraction_Right;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
                return Fraction_Bottom;
            }
        }
        protected override ImageScaleType ImageScale
        {
            get
            {
                return IncomingImageScale;
            }
        }
        /*For ImageProcessBase Information*/

        // Signal of stopping running
        private bool IsStopRunning = false;
        private Stopwatch stopwatch;
        private uint NewFrameCount = 0;
        // Record Timestamp 

        /// <summary>
        /// Create a Image Processor to receive the frame from CaptureCard/Texture2D.
        /// </summary>
        /// <param name="Fraction_Left">The left border to clip incoming image.</param>
        /// <param name="Fraction_Right">The right border to clip incoming image.</param>
        /// <param name="Fraction_Top">The top border to clip incoming image.</param>
        /// <param name="Fraction_Bottom">The bottom border to clip incoming image.</param>
        /// <param name="ImageScale">The scale of incoming image.</param>
        public ImageProcess(double Fraction_Left, double Fraction_Right, double Fraction_Top, double Fraction_Bottom, ImageScaleType ImageScale)
            : this(Fraction_Left, Fraction_Right, Fraction_Top, Fraction_Bottom, ImageScale, -1)
        {

        }
        /// <summary>
        /// Create a Image Processor to receive the frame from CaptureCard/Texture2D.
        /// </summary>
        /// <param name="Fraction_Left">The left border to clip incoming image.</param>
        /// <param name="Fraction_Right">The right border to clip incoming image.</param>
        /// <param name="Fraction_Top">The top border to clip incoming image.</param>
        /// <param name="Fraction_Bottom">The bottom border to clip incoming image.</param>
        /// <param name="ImageScale">The scale of incoming image.</param>
        /// <param name="FrameRate">The frame rate that application requires. Use negative number to represent infinity.</param>
        public ImageProcess(double Fraction_Left = 0, double Fraction_Right = 1, double Fraction_Top = 0, double Fraction_Bottom = 1, ImageScaleType ImageScale = ImageScaleType.OriginalSize, int FrameRate = 1)
            :base()
        {
            this.Fraction_Left = Fraction_Left;
            this.Fraction_Right = Fraction_Right;
            this.Fraction_Top = Fraction_Top;
            this.Fraction_Bottom = Fraction_Bottom;
            IncomingImageScale = ImageScale;
            PreferFrameRate = FrameRate;

            // Frame Rate Setup
            if (FrameRate > 0)
                stopwatch = new Stopwatch();
            stopwatch?.Start();

        }
        ~ImageProcess()
        {
            IsStopRunning = true;
        }
        private static int CalcFrameRate(long TotalFrameCount, long ElaspedMillsecond)
        {
            return (int)((TotalFrameCount * 1000) / ElaspedMillsecond);
        }
        protected override void ImageHandler(object args)
        {
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);

                // Check FrameRate
                if (PreferFrameRate >= 0 && CalcFrameRate(NewFrameCount, stopwatch.ElapsedMilliseconds) >= PreferFrameRate)
                    return;
                // Invoke Event
                NewFrameArrivedEvent?.Invoke(Data);

                // Add FrameCount
                ++NewFrameCount;
                
                // Release Lock to fetch new frame
                IsProcessingData = false;
            }
        }

    }

}
