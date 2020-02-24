using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV.Structure;
using Emgu.CV.Util;
namespace WPFCaptureSample.ScreenCapture.ImageProcess
{
    class OpticalFlow : ImageProcessBase
    {
        protected override double Scale_Width
        {
            get
            {
                return 0.5f;
            }
        }
        protected override double Scale_Height
        {
            get
            {
                return 0.5f;
            }
        }
        private bool IsStopRunning = false;
        private Mat LastImg = null;
        ~OpticalFlow()
        {
            IsStopRunning = true;
        }

        private void featureTracking(Mat img_1, Mat img_2, ref PointF[] points1, out PointF[] points2, out byte[] status)
        {
            //this function automatically gets rid of points for which tracking fails
            float[] err;
            Size winSize = new Size(21, 21);
            MCvTermCriteria termcrit = new MCvTermCriteria(30, 0.01);
            CvInvoke.CalcOpticalFlowPyrLK(img_1, img_2, points1, winSize, 3, new MCvTermCriteria(20, 0.03), out points2, out status, out err);
            //getting rid of points for which the KLT tracking failed or those who have gone outside the frame
            int indexCorrection = 0;
            for (int i = 0; i < status.Length; i++)
            {
                PointF pt = points2.ElementAt(i - indexCorrection);
                if ((status.ElementAt(i) == 0) || (pt.X < 0) || (pt.Y < 0))
                {
                    if ((pt.X < 0) || (pt.Y < 0))
                    {
                        status.SetValue(0, i);
                    }
                    points1.At(points1.begin() + (i - indexCorrection));
                    points2.erase(points2.begin() + (i - indexCorrection));
                    indexCorrection++;
                }

            }

        }
        protected override void ImageHandler(object args)
        {
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                if (LastImg == null)
                {
                    LastImg = new Mat(Data.Size, Data.Depth, Data.NumberOfChannels);
                    goto EndOfProcess;
                }
                CvInvoke.Imwrite("O:\\Test.png", Data);
                return;
                EndOfProcess:
                IsProcessingData = false;
            }
        }

    }
}
