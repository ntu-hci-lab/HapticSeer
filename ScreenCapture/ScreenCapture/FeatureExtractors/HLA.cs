﻿using System;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using ImageProcessModule.ProcessingClass;
using Tesseract;

using static ImageProcessModule.ImageProcessBase;

namespace ScreenCapture
{
    class HLA : FeatureExtractors
    {
        public HLA() : base()
        {
            //ImageProcessesList.Add(new ImageProcess(64 / 1920f, 302 / 1920f, 956 / 1080f, 1015 / 1080f, ImageScaleType.OriginalSize, FrameRate: 10));
            //ImageProcessesList.Last().NewFrameArrivedEvent += BloodDetectorEvent;

            //ImageProcessesList.Add(new ImageProcess(1700 / 1920f, 1777 / 1920f, 955 / 1080f, 1015 / 1080f, ImageScaleType.OriginalSize, FrameRate: 3));
            ImageProcessesList.Add(new ImageProcess(2263 / 2560f, 2375 / 2560f, 1350 / 1600f, 1430 / 1600f, ImageScaleType.OriginalSize));
            ImageProcessesList.Last().NewFrameArrivedEvent += BulletInGunEvent;

            //ImageProcessesList.Add(new ImageProcess(1796 / 1920f, 1859 / 1920f, 986 / 1080f, 1015 / 1080f, ImageScaleType.OriginalSize, FrameRate: 3));
            //ImageProcessesList.Last().NewFrameArrivedEvent += BulletInBackpackEvent;
        }
        private static void BulletInBackpackEvent(ImageProcess sender, Mat mat)
        {
            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));

            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(250, 0, 0) }, new uint[] { 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 70);
            // Tesseract OCR
            Pix pixImage;
            Page page;
            try
            {
                pixImage = PixConverter.ToPix(BinaryImg.ToBitmap());
                page = ocr_eng.Process(pixImage, PageSegMode.SingleBlock);
                var bulletStr = page.GetText();
                page.Dispose();
                pixImage.Dispose();
                if (!string.IsNullOrEmpty(bulletStr))
                {
                    if (Int32.TryParse(bulletStr, out int num))
                    {
                        publisher.Publish("BULLET", num.ToString());
#if DEBUG
                        Console.WriteLine($"Bullet in backpack: {num}\t");
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error message: " + ex.Message);
            }
        }

        private static void BloodDetectorEvent(ImageProcess sender, Mat mat)
        {
            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));

            if (!sender.Variable.ContainsKey("LowPassFilter_Blood"))
                sender.Variable.Add("LowPassFilter_Blood", (double)1);

            if (!sender.Variable.ContainsKey("BloodArea"))
            {
                double _BloodArea = mat.Height * mat.Width * 0.343;
                sender.Variable.Add("BloodArea", _BloodArea);
            }
            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            double LowPassFilter_Blood = (double)sender.Variable["LowPassFilter_Blood"];
            double BloodArea = (double)sender.Variable["BloodArea"];
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

        private static void BulletInGunEvent(ImageProcess sender, Mat mat)
        {
            if (!sender.Variable.ContainsKey("BinaryImage"))
                sender.Variable.Add("BinaryImage", new Mat(mat.Size, DepthType.Cv8U, 1));

            Mat BinaryImg = sender.Variable["BinaryImage"] as Mat;
            ImageProcess.ElimateBackgroundWithSearchingSimilarColor(in mat, ref BinaryImg, new Color[] { Color.FromArgb(250, 0, 0) }, new uint[] { 0x00FF0000 }, ElimateColorApproach.ReserveSimilarColor_RemoveDifferentColor, 70);
            // Tesseract OCR
            Pix pixImage;
            Page page;
            try
            {
                pixImage = PixConverter.ToPix(BinaryImg.ToBitmap());
                page = ocr_eng.Process(pixImage, PageSegMode.SingleBlock);
                var bulletStr = page.GetText();
                page.Dispose();
                pixImage.Dispose();
                if (!string.IsNullOrEmpty(bulletStr))
                {
                    if (Int32.TryParse(bulletStr, out int num))
                    {
                        publisher.Publish("BULLET", num.ToString());
#if DEBUG
                        Console.WriteLine($"Bullet in gun: {num}\t");
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error message: " + ex.Message);
            }
        }

    }
}
