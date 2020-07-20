using Emgu.CV;
using Emgu.CV.CvEnum;
using ImageProcessModule;
using ImageProcessModule.ProcessingClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Tesseract;
using RedisEndpoint;
using static ImageProcessModule.ImageProcessBase;

namespace ScreenCapture
{
    class Program
    {
        /// <summary>
        /// Specify the scenario that the program runs
        /// </summary>
        public enum GameType
        {
            None,
            HL_A,
            Project_Cars,
            BF1
        }
        static GameType RunningGameType = GameType.Project_Cars;
        static BitmapBuffer bitmapBuffer = new BitmapBuffer();
        static CaptureMethod captureMethod;

        /// Initialize RedisEndpoint
        static Publisher publisher = new Publisher("localhost", 6380);
        

        /// Initialize Tesseract object
        /// Remember to add tessdata directory
        static TesseractEngine ocr = new TesseractEngine(Path.GetFullPath(@"..\..\"), "eng", EngineMode.Default);
        static KalmanFilter filter = new KalmanFilter(1, 1, 0.05, 1, 0.1, speed);
        static SpeedImageProcess speedImageProcess = new SpeedImageProcess();
        static int speed = 0; // current speed
        static int preSpeed = 0; // previous speed
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
            //args = new string[] { "CaptureCard" };
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

            List<ImageProcess> ImageProcesses = new List<ImageProcess>();
            switch (RunningGameType)
            {
                case GameType.None:
                    break;
                case GameType.HL_A:
                    ImageProcess BloodDetector_HLA = new ImageProcess(64 / 1920f, 302 / 1920f, 956 / 1080f, 1015 / 1080f, ImageScaleType.OriginalSize, FrameRate: 10);
                    BloodDetector_HLA.NewFrameArrivedEvent += BloodDetector_HLA_NewFrameArrivedEvent;
                    ImageProcess BulletInGun_HLA = new ImageProcess(1700 / 1920f, 1777 / 1920f, 955 / 1080f, 1015 / 1080f, ImageScaleType.OriginalSize, FrameRate: 3);
                    BulletInGun_HLA.NewFrameArrivedEvent += BulletInGun_HLA_NewFrameArrivedEvent;
                    ImageProcess BulletInBackpack_HLA = new ImageProcess(1796 / 1920f, 1859 / 1920f, 986 / 1080f, 1015 / 1080f, ImageScaleType.OriginalSize, FrameRate: 3);
                    BulletInBackpack_HLA.NewFrameArrivedEvent += BulletInBackpack_HLA_NewFrameArrivedEvent;
                    ImageProcesses.Add(BloodDetector_HLA);
                    ImageProcesses.Add(BulletInGun_HLA);
                    ImageProcesses.Add(BulletInBackpack_HLA);
                    break;
                case GameType.Project_Cars:
                    // Speed detection
                    ImageProcess SpeedDetection = new ImageProcess(1541 / 1720f, 1601 / 1720f, 865d / 1080, 903d / 1080, ImageProcessBase.ImageScaleType.OriginalSize);
                    SpeedDetection.NewFrameArrivedEvent += SpeedDetection_NewFrameArrivedEvent;
                    ImageProcesses.Add(SpeedDetection);
                    break;
                case GameType.BF1:
                    ImageProcess DamageIndicatorDetection = new ImageProcess(0, 1, 0, 1, ImageProcessBase.ImageScaleType.Quarter);
                    DamageIndicatorDetection.NewFrameArrivedEvent += DamageIndicatorDetection_NewFrameArrivedEvent;

                    ImageProcess BloodDetector_BF1 = new ImageProcess(1500 / 1728f, 1700 / 1728f, 1028 / 1080f, 1029 / 1080f, ImageScaleType.OriginalSize, FrameRate: 15);
                    BloodDetector_BF1.NewFrameArrivedEvent += BloodDetector_BF1_NewFrameArrivedEvent;
                    ImageProcess BulletCount_BF1 = new ImageProcess(1526 / 1728f, 1594 / 1728f, 948 / 1080f, 988 / 1080f, ImageScaleType.OriginalSize, FrameRate: 3);
                    BulletCount_BF1.NewFrameArrivedEvent += BulletCount_BF1_NewFrameArrivedEvent;
                    ImageProcess GrenadeCount_BF1 = new ImageProcess(1598 / 1728f, 1628 / 1728f, 978 / 1080f, 998 / 1080f, ImageScaleType.OriginalSize, FrameRate: 3);
                    GrenadeCount_BF1.NewFrameArrivedEvent += GrenadeCount_BF1_NewFrameArrivedEvent;
                    ImageProcesses.Add(DamageIndicatorDetection);
                    ImageProcesses.Add(BloodDetector_BF1);
                    ImageProcesses.Add(BulletCount_BF1);
                    ImageProcesses.Add(GrenadeCount_BF1);
                    break;
            }
            // Do Cache Optimizer
            CacheOptimizer.Init();
            CacheOptimizer.ResetAllAffinity();
        }

        private static void GrenadeCount_BF1_NewFrameArrivedEvent(ImageProcess sender, Mat mat)
        {
            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));
            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(220, 220, 220) }, new uint[] { 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 50);
            // TODO OCR to BinaryImg
        }

        private static void BulletCount_BF1_NewFrameArrivedEvent(ImageProcess sender, Mat mat)
        {
            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));
            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(220, 220, 220) }, new uint[] { 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 50);
            // TODO OCR to BinaryImg
        }

        private static void BulletInBackpack_HLA_NewFrameArrivedEvent(ImageProcess sender, Mat mat)
        {
            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));

            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(250, 0, 0) }, new uint[] { 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 70);
            // TODO OCR to BinaryImg
        }

        private static void BulletInGun_HLA_NewFrameArrivedEvent(ImageProcess sender, Mat mat)
        {
            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));

            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(250, 0, 0) }, new uint[] { 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 70);
            // TODO OCR to BinaryImg
        }

        private static void BloodDetector_BF1_NewFrameArrivedEvent(ImageProcess sender, Mat mat)
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

        private static void BloodDetector_HLA_NewFrameArrivedEvent(ImageProcess sender, Mat mat)
        {
            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));

            if (!sender.Variable.ContainsKey("LowPassFilter_Blood"))
                sender.Variable.Add("LowPassFilter_Blood", (double)1);

            if (!sender.Variable.ContainsKey("BloodArea"))
            {
                double _BloodArea = mat.Height * mat.Width * 0.343;
                //double _BloodArea = mat.Height * mat.Width * 0.3164556962025316;
                sender.Variable.Add("BloodArea", _BloodArea);
            }
            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            double LowPassFilter_Blood = (double)sender.Variable["LowPassFilter_Blood"];
            double BloodArea = (double)sender.Variable["BloodArea"];
            //ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(235, 189, 0), Color.FromArgb(190, 189, 0), Color.FromArgb(220, 0, 0) }, new uint[] { 0x00FFFF00, 0x00FFFF00, 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 20);
            ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(250, 0, 0) }, new uint[] { 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 70);
            double NowBlood;
            unsafe
            {
                byte* BinaryImageByteArray = (byte*)BinaryImg.DataPointer;
                int Offset = 0;
                int Area = 0;
                for (int y = 0; y < mat.Height; ++y)
                {
                    for (int x = 0; x < mat.Width; ++x)
                    {
                        if (BinaryImageByteArray[Offset++] > 0)
                            Area++;
                    }
                }
                NowBlood = Area / BloodArea;
            }
            double EstimatedBlood = NowBlood * 0.2 + LowPassFilter_Blood * 0.8;
#if DEBUG
            Console.WriteLine($"Actual: {NowBlood.ToString("0.000")}\t Filted: {EstimatedBlood.ToString("0.000")}");
#endif
            sender.Variable["LowPassFilter_Blood"] = EstimatedBlood;
        }
        
        private static void SpeedDetection_NewFrameArrivedEvent(ImageProcess sender, Mat mat)
        {
            /* declare variables for Tesseract */
            Pix pixImage;
            Page page;

            try
            {
                Bitmap BitmapFrame = mat.ToBitmap();
                /* image processing */
                speedImageProcess.ToBlackWhite(BitmapFrame); // grayscale(black and white)
                // BitmapFrame = speedImageProcess.NegativePicture(BitmapFrame); //turn into negative image
                speedImageProcess.ResizeImage(BitmapFrame, 120, 76); // enlarge image(x2)

                pixImage = PixConverter.ToPix(BitmapFrame); // PixConverter is unable to work at Tesseract 3.3.0
                page = ocr.Process(pixImage);
                string speedStr = page.GetText(); // Recognized result
                page.Dispose();
                pixImage.Dispose();

                ///* Parse str to int */
                bool isParsable = Int32.TryParse(speedStr, out speed);
                if (!isParsable)
                {
                    // Console.WriteLine("Speed could not be parsed.");
                    speed = preSpeed; // Can't detect speed, use the previous speed value
                }

                /* Prevent negative number or large number */
                if (speed < 0 || speed > 300)
                    speed = preSpeed;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error message: " + ex.Message);
            }
            // Console.WriteLine("  -Current speed: " + speed + " mph");
            preSpeed = speed;

            /* Filtering(denoise) */
            speed = (int)filter.Output(speed);
            publisher.Publish("SPEED", $"SMOOTHED|{speed}\n");
            Console.WriteLine("  -Smoothed speed: " + speed + " mph\n");
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
