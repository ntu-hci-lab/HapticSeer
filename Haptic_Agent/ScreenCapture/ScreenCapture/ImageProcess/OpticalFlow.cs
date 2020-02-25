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
using Emgu.CV.Features2D;

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
        private PointF[] LastImgFeatures;
        ~OpticalFlow()
        {
            IsStopRunning = true;
        }

        private void FastFeatureDetection(Mat img, out PointF[] features)
        {
            using (FastFeatureDetector fastFeatureDetector = new FastFeatureDetector(50))
            {
                MKeyPoint[] KeyPoints = fastFeatureDetector.Detect(img);
                features = KeyPoints.Select(x => x.Point).ToArray();
            }
        }

        private void featureTracking(Mat img_1, Mat img_2, ref PointF[] points1, out PointF[] points2, out byte[] status)
        {
            //this function automatically gets rid of points for which tracking fails
            float[] err;
            Size winSize = new Size(21, 21);
            MCvTermCriteria termcrit = new MCvTermCriteria(30, 0.01);
            CvInvoke.CalcOpticalFlowPyrLK(img_1, img_2, points1, winSize, 3, new MCvTermCriteria(20, 0.03), out points2, out status, out err);
        }
        protected override void ImageHandler(object args)
        {
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                PointF[] NewImageFeatures = null;
                byte[] FeatureStatus = null;
                if (LastImg == null)
                {
                    LastImg = new Mat(Data.Size, Data.Depth, Data.NumberOfChannels);
                    goto EndOfProcess;
                }
                else if (!LastImg.Size.Equals(Data.Size))
                    goto EndOfProcess;
                if (LastImgFeatures.Length > 100)
                    featureTracking(LastImg, Data, ref LastImgFeatures, out NewImageFeatures, out FeatureStatus);
                //https://blog.csdn.net/on2way/article/details/48954159

            EndOfProcess:
                Mat temp = LastImg; //Swap Mat
                LastImg = Data;
                Data = temp;
                FastFeatureDetection(LastImg, out LastImgFeatures);
                IsProcessingData = false;
            }
        }

    }
}
