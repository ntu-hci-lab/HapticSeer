using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Threading;

namespace ImageProcessModule
{
    class CircularIndicatorDetector : ImageProcessBase
    {
        private double CircularInnerSizeFractionRelatedToClippedSize = 137 / 231f;
        protected override double Clipped_Left
        {
            get
            {
                return 0.5 - 0.06;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return 0.5 - 0.1065;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return 0.5 + 0.06;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
                return 0.5 + 0.1065;
            }
        }
        protected override ImageScaleType ImageScale
        {
            get
            {
                return ImageScaleType.OriginalSize;
            }
        }

        private volatile bool IsStopRunning = false;
        public CircularIndicatorDetector(double CircularInnerSizeFractionRelatedToClippedSize)
            : base(ImageScaleType.OriginalSize, true, true)
        {
            this.CircularInnerSizeFractionRelatedToClippedSize = CircularInnerSizeFractionRelatedToClippedSize;
        }
        ~CircularIndicatorDetector()
        {
            IsStopRunning = true;
        }

        private unsafe bool IsRedImpulse(byte* Now, byte* Last)
        {
            bool IsRedImpulse = (Last[2] < 127) && (Now[2] > 200);
            if (!IsRedImpulse)
                return false;
            if (Now[0] > 170 || Now[1] > 170)   //Filter White/meaningful color
                return false;
            return true;
        }
        protected override void ImageHandler(object args)
        {
            Mat LastFrame = new Mat();
            Mat RedChannelImg = new Mat();
            int InnerWidth, InnerHeight;
            int InnerStartHeight = 0, InnerStopHeight = 0, InnerStartWidth = 0, InnerStopWidth = 0;
#if DEBUG_IMG_OUTPUT
            int ImgCounter = 0;
#endif
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                if (!LastFrame.Size.Equals(Data.Size))
                {
                    LastFrame.Dispose();
                    RedChannelImg.Dispose();
                    LastFrame = new Mat(Data.Rows, Data.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
                    RedChannelImg = new Mat(Data.Rows, Data.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                    InnerWidth = (int)(Data.Width * CircularInnerSizeFractionRelatedToClippedSize);
                    InnerHeight = (int)(Data.Height * CircularInnerSizeFractionRelatedToClippedSize);
                    InnerStartHeight = (Data.Height - InnerHeight) / 2;
                    InnerStartWidth = (Data.Width - InnerWidth) / 2;
                    InnerStopHeight = Data.Height - InnerStartHeight;
                    InnerStopWidth = Data.Width - InnerStartWidth;
                    goto WaitForNextFrame;
                }

                unsafe
                {
                    byte* CurrentPtr = (byte*)Data.DataPointer;//ARGB
                    byte* PastPtr = (byte*)LastFrame.DataPointer;
                    byte* RedImgPtr = (byte*)RedChannelImg.DataPointer;
                    int Offset = 0;
                    bool IsExistImpulse = false;
                    for (int y = 0; y < Data.Height; ++y)
                    {
                        for (int x = 0; x < Data.Width; ++x)
                        {
                            RedImgPtr[Offset >> 2] = 0;
                            if (InnerStartHeight >= y && InnerStopHeight <= y && InnerStartWidth >= x && InnerStopWidth <= x)
                            {
                                Offset += 4;
                                continue;
                            }
                            if (IsRedImpulse(&CurrentPtr[Offset], &PastPtr[Offset]) && CurrentPtr[Offset + 2] > 200)
                            {
                                RedImgPtr[Offset >> 2] = 255;   //Set As White
                                IsExistImpulse = true;
                            }
                            Offset += 4;
                        }
                    }
                    if (IsExistImpulse)
                    {
                        CvInvoke.MorphologyEx(RedChannelImg, RedChannelImg, Emgu.CV.CvEnum.MorphOp.Open, Kernel_4x4, new System.Drawing.Point(0, 0), 2, BorderType.Default, new MCvScalar(0, 0, 0));
#if DEBUG_IMG_OUTPUT
                        RedChannelImg.Save("O:\\RedChannel" + ImgCounter++ + ".png");
#endif
                    }
                }
                unsafe
                {
                    byte* RedChannelPtr = (byte*)RedChannelImg.DataPointer;
                    int Sum_X = 0, Sum_Y = 0, WhitePixelCounter = 0;
                    for (int y = 0, Offset = 0; y < RedChannelImg.Height; ++y)
                        for (int x = 0; x < RedChannelImg.Width; ++x)
                            if (RedChannelPtr[Offset++] == 255)
                            {
                                Sum_Y += y;
                                Sum_X += x;
                                WhitePixelCounter++;
                            }
                    if (WhitePixelCounter > 16)
                    {
                        double Angle = GetAngleByCircleModel(Sum_X / (double)WhitePixelCounter, Sum_Y / (double)WhitePixelCounter, RedChannelImg.Width, RedChannelImg.Height);
                        Console.WriteLine("Angle: " + Angle);
                    }
                }
            WaitForNextFrame:
                Mat temp = Data;
                Data = LastFrame;
                LastFrame = temp;
                IsProcessingData = false;
            }
        }
    }
}
