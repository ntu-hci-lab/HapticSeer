#define CS_GO
#define DEBUG_IMG_OUTPUT
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace WPFCaptureSample.ScreenCapture.ImageProcess
{
    class BulletCounter : ImageProcessBase
    {
        private bool IsStopRunning = false;
        protected override double Clipped_Left
        {
            get
            {
#if CS_GO
                return 0.8922;
#endif
#if DEBUG_VR
                return 0.269;
#endif
#if DEBUG_RACE
                return 0.905;
#endif
            }
        }
        protected override double Clipped_Top
        {
            get
            {
#if CS_GO
                return 0.9565;
#endif
#if DEBUG_VR
                return 0.774;
#endif
#if DEBUG_RACE
                return 0.905;
#endif
            }
        }
        protected override double Clipped_Right
        {
            get
            {
#if CS_GO
                return 0.947;
#endif
#if DEBUG_VR
                return 0.298;
#endif
#if DEBUG_RACE
                return 0.924;
#endif
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
#if CS_GO
                return 0.988;
#endif
#if DEBUG_VR
                return 0.973;
#endif
#if DEBUG_RACE
                return 0.9249;
#endif
            }
        }
        protected override double Scale_Width
        {
            get
            {
                return 1;
            }
        }
        protected override double Scale_Height
        {
            get
            {
                return 1;
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
