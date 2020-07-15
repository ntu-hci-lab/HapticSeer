using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Threading;

namespace ImageProcessModule
{
    class BulletCounter : ImageProcessBase
    {
        private bool IsStopRunning = false;
        protected override double Clipped_Left
        {
            get
            {
                return 0.8922;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return 0.9565;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return 0.947;
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
        public BulletCounter()
            :base(ImageScaleType.OriginalSize)
        {

        }
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
                ElimateBackgroundWithSimilarItemColor(in Data, ref BackgroundRemovalImage, new Color[] { Color.FromArgb(197, 188, 165) }, 70);
                CvInvoke.MorphologyEx(BackgroundRemovalImage, BackgroundRemovalImage, Emgu.CV.CvEnum.MorphOp.Open, Kernel_2x2, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar(0, 0, 0));

#endif

#if DEBUG_VR
                ElimateBackgroundWithSolidColor(in Data, ref BackgroundRemovalImage, new Color[] { Color.White, Color.Red }, new int[] { ~0, 0xFF << 16 });
                Console.WriteLine(BarLengthCalc(BackgroundRemovalImage, 4, true));
#endif
#if DEBUG_RACE
                ElimateBackgroundWithSimilarItemColor(in Data, ref BackgroundRemovalImage, new Color[] { Color.White }, 70);
#endif

#if DEBUG_IMG_OUTPUT
                CvInvoke.Imwrite("O:\\Bullet_Ori.png", Data);
                CvInvoke.Imwrite("O:\\Bullet_Out.png", BackgroundRemovalImage);
#endif

                IsProcessingData = false;
            }
        }
    }
}
