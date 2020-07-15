using Emgu.CV;
using Emgu.CV.CvEnum;
using ImageProcessModule;
using ImageProcessModule.ProcessingClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenCapture
{
    class Program
    {
        static BitmapBuffer bitmapBuffer = new BitmapBuffer();
        static CaptureMethod captureMethod;
        /// <summary>
        /// Parse Argument to get the Capture Method
        /// </summary>
        /// <param name="args">Args from Main</param>
        static void ArgumentParser(string[] args)
        {
            if (String.Compare(args[0], "Local", StringComparison.OrdinalIgnoreCase) == 0)
                captureMethod = new LocalCapture(bitmapBuffer); //Default: Local Capture
            else if (String.Compare(args[0], "CaptureCard", StringComparison.OrdinalIgnoreCase) == 0)
                captureMethod = new CardCapture(bitmapBuffer);  //Fetch Image From Capture Card
            else
            {
                // Unknown Argument
                Console.WriteLine();
                Console.WriteLine("Wrong Argument!");
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{Process.GetCurrentProcess().ProcessName} [ImageSource]");
                Console.WriteLine($"\t[ImageSource]: Local or CaptureCard");
                Console.WriteLine();
                // Force close the process
                Process.GetCurrentProcess().Kill();
            }
        }
        static void Main(string[] args)
        {
            //Thread.Sleep(5000);
            Console.CancelKeyPress +=
                new ConsoleCancelEventHandler((o, t) =>
                {
                    captureMethod.Stop();   //Stop Capturing
                });

            // Check the Capture Method from args
            if (args.Length == 0)
                captureMethod = new LocalCapture(bitmapBuffer); // Default: Local Capture
            else
                ArgumentParser(args); // Parse Arguments

            if (captureMethod == null)
                throw new Exception("Error! CaptureMethod is null!");

            // Start Capture
            captureMethod.Start();

            // Start dispatch frames
            bitmapBuffer.StartDispatchToImageProcessBase();
            ImageProcess DamageIndicatorDetection = new ImageProcess(0, 1, 0, 1, ImageProcessBase.ImageScaleType.Quarter);
            DamageIndicatorDetection.NewFrameArrivedEvent += DamageIndicatorDetection_NewFrameArrivedEvent;

            //BarBloodIndicatorDetector barBloodIndicatorDetector = new BarBloodIndicatorDetector();

            // Do Cache Optimizer
            CacheOptimizer.Init();
            CacheOptimizer.ResetAllAffinity();
        }

        private static void DamageIndicatorDetection_NewFrameArrivedEvent(ImageProcess sender, Mat mat)
        {
            Mat LastImg, AvailableImg;
            if (sender.Variable.ContainsKey("LastImg"))
            {
                LastImg = sender.Variable["LastImg"] as Mat;
                AvailableImg = sender.Variable["AvailableImg"] as Mat;
            }else
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
                if (GetHitAngle(LastImg, out angle))
                    Console.WriteLine(angle);
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
