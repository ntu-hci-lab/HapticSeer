using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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
                return 0.056;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return 0.9787;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return 0.097;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
                return 0.988;
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

        protected override void ImageHandler(object args)
        {
            MCvScalar scalar = new MCvScalar(0);
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                if (!BackgroundRemovalImage.Size.Equals(Data.Size))
                {
                    BackgroundRemovalImage.Dispose();
                    BackgroundRemovalImage = new Mat(Data.Size, DepthType.Cv8U, 1);
                }
                BackgroundRemovalImage.SetTo(scalar);
#if CS_GO
                ElimateBackgroundWithSolidColor(in Data, ref BackgroundRemovalImage, new Color[] { Color.FromArgb(155, 153, 122), Color.FromArgb(188, 56, 0) }, new uint[] { 0xC0C0C0C0, 0xC0C0C0C0 });
                Console.WriteLine(BarLengthCalc(BackgroundRemovalImage, 4, false));
#endif
                IsProcessingData = false;
            }
        }
    }
}
