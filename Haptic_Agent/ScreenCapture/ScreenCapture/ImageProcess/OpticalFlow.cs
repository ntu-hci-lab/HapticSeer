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
        protected override double Clipped_Left
        {
            get
            {
                return 0;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return 0.3f;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return 1;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
                return 0.8f;
            }
        }
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
        private double FrameRate = 10;
        private bool IsStopRunning = false;
        private Mat LastImg = null;
        private PointF[] LastImgFeatures;
        ~OpticalFlow()
        {
            IsStopRunning = true;
        }
        private bool TestForward(ref double[,] Avg_Y_Displacement, ref long[,] Count)
        {
            bool IsForward = false;
            int temp_Score = 0;
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (Avg_Y_Displacement[i, j] < -2)
                    {
                        IsForward = true;
                        temp_Score += 3 * i + j;
                    }else if (Count[i, j] < 5)
                    {
                        temp_Score += 3 * i + j;
                    }
                }
            }
            if (IsForward && temp_Score > 20)
                return true;
            else
                return false;
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
            CvInvoke.CalcOpticalFlowPyrLK(img_1, img_2, points1, winSize, 3, termcrit, out points2, out status, out err);
        }
        protected override void ImageHandler(object args)
        {
            double TicksPerFrame = 1000 / FrameRate * TimeSpan.TicksPerMillisecond;
            double NextFrameAcceptTime = -1;
            long[,] X_Displacement = new long[3, 3];
            long[,] Y_Displacement = new long[3, 3];
            long[,] Count = new long[3, 3];
            double[,] Avg_X_Displacement = new double[3, 3];
            double[,] Avg_Y_Displacement = new double[3, 3];
            while (!IsStopRunning)
            {
                Array.Clear(X_Displacement, 0, X_Displacement.Length);
                Array.Clear(Y_Displacement, 0, Y_Displacement.Length);
                Array.Clear(Count, 0, Count.Length);
                while (!IsProcessingData)
                    Thread.Sleep(1);
//                CvInvoke.Imwrite("O:\\Out.png", Data);
                if (NextFrameAcceptTime < 0)
                    NextFrameAcceptTime = DateTime.Now.Ticks;
                else if (NextFrameAcceptTime > DateTime.Now.Ticks)
                {
                    IsProcessingData = false;
                    continue;
                }else
                {
                    NextFrameAcceptTime += TicksPerFrame;
                }

                PointF[] NewImageFeatures = null;
                byte[] FeatureStatus = null;
                if (LastImg == null)
                {
                    LastImg = new Mat(Data.Size, Data.Depth, Data.NumberOfChannels);
                    goto EndOfProcess;
                }
                else if (!LastImg.Size.Equals(Data.Size))
                    goto EndOfProcess;
                if (LastImgFeatures.Length < 20)
                {
                    Console.WriteLine("Failed " + LastImgFeatures.Length);
                    goto EndOfProcess;
                }
                featureTracking(LastImg, Data, ref LastImgFeatures, out NewImageFeatures, out FeatureStatus);
                for (int i = 0, size = FeatureStatus.Length; i < size; ++i)
                {
                    if (FeatureStatus[i] == 0 || NewImageFeatures[i].X < 0 || NewImageFeatures[i].Y < 0)
                        continue;
                    float prev_x = LastImgFeatures[i].X,
                        prev_y = LastImgFeatures[i].Y;
                    int prev_x_cluster = (int)(prev_x / LastImg.Cols * 3),
                        prev_y_cluster = (int)(prev_y / LastImg.Rows * 3);
                    int DeltaX = (int)(NewImageFeatures[i].X - LastImgFeatures[i].X),
                        DeltaY = (int)(NewImageFeatures[i].Y - LastImgFeatures[i].Y);
                    if (Math.Abs(DeltaX) + Math.Abs(DeltaY) < 5)
                        continue;
                    Count[prev_x_cluster, prev_y_cluster]++;
                    X_Displacement[prev_x_cluster, prev_y_cluster] += DeltaX;
                    Y_Displacement[prev_x_cluster, prev_y_cluster] += DeltaY;
                }
                long Total_X_Displacement = 0;
                long X_Counter = 0;
                for (int i = 0; i < 3; ++i)
                {
                    for (int j = 0; j < 3;  ++j)
                    {
                        if (Count[i, j] != 0)
                        {
                            Avg_X_Displacement[i, j] = X_Displacement[i, j] / Count[i, j];
                            Total_X_Displacement += X_Displacement[i, j];
                            X_Counter += Count[i, j];
                            Avg_Y_Displacement[i, j] = Y_Displacement[i, j] / Count[i, j];
                        }
                        else
                        {
                            Avg_X_Displacement[i, j] = Avg_Y_Displacement[i, j] = 0;
                        }
                    }
                }
                bool IsForward = TestForward(ref Avg_Y_Displacement, ref Count);
                long Avg_X_Movement;
                if (X_Counter != 0)
                    Avg_X_Movement = Total_X_Displacement / X_Counter;
                else
                    Avg_X_Movement = 0;
                if (IsForward)
                {
                    if (Avg_X_Movement >= 3)
                        Console.WriteLine("Left " + LastImgFeatures.Length);
                    else if (Avg_X_Movement <= -3)
                        Console.WriteLine("Right " + LastImgFeatures.Length);
                    else
                        Console.WriteLine("Straight " + LastImgFeatures.Length);
                }
            EndOfProcess:
                Mat temp = LastImg; //Swap Mat
                LastImg = Data;
                Data = temp;
                IsProcessingData = false;
                FastFeatureDetection(LastImg, out LastImgFeatures);
            }
        }

    }
}
