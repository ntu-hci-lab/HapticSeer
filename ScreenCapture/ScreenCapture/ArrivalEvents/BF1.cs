using System;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using ImageProcessModule.ProcessingClass;
using Tesseract;

using static ImageProcessModule.ImageProcessBase;

namespace ScreenCapture
{
    class BF1 : ArrivalEvent
    {
        
        public BF1(): base()
        {
            ImageProcessesList.Add(new ImageProcess(0, 1, 0, 1, ImageScaleType.Quarter));
            ImageProcessesList.Last().NewFrameArrivedEvent += DamageIndicatorDetectionEvent;

            ImageProcessesList.Add(new ImageProcess(1689 / 1920f, 1867 / 1920f, 1018 / 1080f, 1020 / 1080f, ImageScaleType.OriginalSize, FrameRate: 15));
            ImageProcessesList.Last().NewFrameArrivedEvent += BloodDetectorEvent;

            //ImageProcess BulletCount_BF1 = new ImageProcess(1526 / 1728f, 1594 / 1728f, 948 / 1080f, 988 / 1080f, ImageScaleType.OriginalSize, FrameRate: 3);
            ImageProcessesList.Add (new ImageProcess(0.89, 0.922, 0.865, 0.93, ImageScaleType.OriginalSize, FrameRate: 30)); // 1920*1080
            ImageProcessesList.Last().NewFrameArrivedEvent += BulletCountEvent;

            //ImageProcessesList.Add(new ImageProcess(1598 / 1728f, 1628 / 1728f, 978 / 1080f, 998 / 1080f, ImageScaleType.OriginalSize, FrameRate: 3));
            //ImageProcessesList.Last().NewFrameArrivedEventt += GrenadeCount_BF1_NewFrameArrivedEvent;
        }

        private static void GrenadeCountEvent(ImageProcess sender, Mat mat)
        {
            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));
            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(220, 220, 220) }, new uint[] { 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 50);
        }

        private static void BulletCountEvent(ImageProcess sender, Mat mat)
        {
            Pix pixImage;
            Page page;

            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));
            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(220, 220, 220) }, new uint[] { 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 50);

            /* use Tesseract to recognize number */
            try
            {
                pixImage = PixConverter.ToPix(BinaryImg.ToBitmap());
                ocr.DefaultPageSegMode = PageSegMode.SingleBlock;
                page = ocr.Process(pixImage);
                var bulletStr = page.GetText(); // 識別後的內容
                if (!string.IsNullOrEmpty(bulletStr))
                {
                    //Save(BinaryImg, DateTime.Now.Ticks.ToString());

                    if (Int32.TryParse(bulletStr, out int num))
                    {
                        Console.WriteLine("bullet number :" + num.ToString());
                    }
                }

                page.Dispose();
                pixImage.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error message: " + ex.Message);
            }
        }


        private static void BloodDetectorEvent(ImageProcess sender, Mat mat)
        {
            double BloodValue;
            bool IsRedImpluse = false;
            unsafe
            {
                byte* OriginalImageByteArray = (byte*)mat.DataPointer;

                int Offset = 0;
                int Area = 0;
                //Only read the first row
                for (int x = 0; x < mat.Width; ++x)
                {
                    //White
                    if (OriginalImageByteArray[Offset + 2] > 180 && OriginalImageByteArray[Offset] > 180)
                        Area++;

                    //Red
                    else if (OriginalImageByteArray[Offset + 2] > 120)
                    {
                        //Is Leftmost?
                        if (x != 4)
                            Area++;
                        else if (x == 4)//Left shouldn't be red. It must be impluse
                            IsRedImpluse = true;
                    }
                    Offset += 4;
                }
                BloodValue = Area / (double)(mat.Width);
            }

#if DEBUG
            if (!IsRedImpluse)
                Console.WriteLine($"Actual: {BloodValue.ToString("0.000")}");
#endif
        }




        private static void DamageIndicatorDetectionEvent(ImageProcess sender, Mat mat)
        {
            Mat LastImg, AvailableImg;
            if (sender.Variable.ContainsKey("LastImg"))
            {
                LastImg = sender.Variable["LastImg"] as Mat;
                AvailableImg = sender.Variable["AvailableImg"] as Mat;
            }
            else
            {
                LastImg = new Mat(mat.Rows, mat.Cols, DepthType.Cv8U, 1);
                AvailableImg = new Mat(mat.Rows, mat.Cols, DepthType.Cv8U, 1);
            }

            unsafe
            {
                byte* Input = (byte*)mat.DataPointer;
                byte* Output = (byte*)AvailableImg.DataPointer;
                byte* LastImage = (byte*)LastImg.DataPointer;
                int Offset = 0;
                for (int i = 0; i < AvailableImg.Rows; ++i)
                {
                    for (int j = 0; j < AvailableImg.Cols; ++j)
                    {
                        int Green_Blue_Average = (Input[Offset] + Input[Offset + 1]) / 2;
                        int Red = Input[Offset + 2];
                        int Diff = Math.Max(Red - Green_Blue_Average, 0);
                        Output[Offset / 4] = (byte)Diff;

                        if (Diff <= 180)
                            LastImage[Offset / 4] = 0;
                        else
                            LastImage[Offset / 4] = (byte)(Diff - LastImage[Offset / 4]);
                        Offset += 4;
                    }
                }

                double angle;
                //Console.Clear();
                //if (GetHitAngle(LastImg, out angle))
                //    Console.WriteLine(angle);
                //else
                //    Console.WriteLine("false");
                CvInvoke.Blur(AvailableImg, AvailableImg, new Size(5, 5), new Point(0, 0));
                sender.Variable["LastImg"] = AvailableImg;
                sender.Variable["AvailableImg"] = LastImg;
            }
        }
        private static bool GetHitAngle(Mat OneChannelImg, out double Angle)
        {
            long Sum_X = 0, Sum_Y = 0;
            int CenterX = OneChannelImg.Width / 2,
                CenterY = OneChannelImg.Height / 2;
            unsafe
            {
                byte* OneChannelImgByteArray = (byte*)OneChannelImg.DataPointer;
                int Offset = 0;
                for (int y = 0; y < OneChannelImg.Height; ++y)
                {
                    int _y = CenterY - y;
                    for (int x = 0; x < OneChannelImg.Width; ++x)
                    {
                        uint Value = OneChannelImgByteArray[Offset++];
                        int _x = x - CenterX;
                        Sum_X += Value * _x;
                        Sum_Y += Value * _y;
                    }
                }
            }
            double SumY_Fraction = Sum_Y / (double)OneChannelImg.Width;
            double SumX_Fraction = Sum_X / (double)OneChannelImg.Height;
            if (SumX_Fraction == 0 && SumY_Fraction == 0)
            {
                Angle = 0;
                return false;
            }
            var _angle = Math.Acos(SumX_Fraction / Math.Sqrt(SumX_Fraction * SumX_Fraction + SumY_Fraction * SumY_Fraction)) * 180 / Math.PI;
            if (SumY_Fraction > 0)
                Angle = _angle;
            else
                Angle = (360 - _angle);
            return true;
        }

    }
}
