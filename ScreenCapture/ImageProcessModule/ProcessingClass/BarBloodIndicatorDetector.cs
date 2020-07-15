using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Threading;

namespace ImageProcessModule.ProcessingClass
{
    public class BarBloodIndicatorDetector : ImageProcessBase
    {
        private bool IsStopRunning = false;
        protected override double Clipped_Left
        {
            get
            {
                return 120 / 1920f;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return 1052 / 1080f;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return 202 / 1920f;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
                return 1062 / 1080f;
            }
        }
        protected override ImageScaleType ImageScale
        {
            get
            {
                return ImageScaleType.OriginalSize;
            }
        }
        private Mat BackgroundRemovalImage = new Mat();
        public BarBloodIndicatorDetector()
            :base(ImageScaleType.OriginalSize)
        {

        }
        protected override void ImageHandler(object args)
        {
            MCvScalar scalar = new MCvScalar(0);
            // Check is the class still alive
            while (!IsStopRunning)
            {
                // Check is data updating
                while (!IsProcessingData)
                    Thread.Sleep(1);

                // Check the size of binary image is equal to Data.Size 
                if (!BackgroundRemovalImage.Size.Equals(Data.Size))
                {
                    BackgroundRemovalImage.Dispose();
                    BackgroundRemovalImage = new Mat(Data.Size, DepthType.Cv8U, 1);
                }
                // Clean all data in BackgroundRemovalImage
                BackgroundRemovalImage.SetTo(scalar);

                ElimateBackgroundWithSearchingSimilarColor(in Data, ref BackgroundRemovalImage, new Color[] { Color.FromArgb(235, 235, 235), Color.FromArgb(188, 56, 0) }, new uint[] { 0x00FFFFFF, 0x00FFFFFF }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 8);
                //Data.Save("O:\\Raw.png");
                //BackgroundRemovalImage.Save("O:\\Test.png");
                double BarLength = BarLengthCalc(BackgroundRemovalImage, 4, false);
                //Console.WriteLine(BarLengthCalc(BackgroundRemovalImage, 4, false));

                IsProcessingData = false;
            }
        }
    }
}
