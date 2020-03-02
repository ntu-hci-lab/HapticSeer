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
    class ScreenClipped : ImageProcessBase
    {
        private bool IsStopRunning = false;
        protected override double Clipped_Left
        {
            get
            {
                return 0.269;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return 0.774;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return 0.298;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
                return 0.973;
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
        private static unsafe double BarLengthCalc(in Mat BinaryImage, in int WidthRequest, in bool IsPortrait)
        {
            byte* ImgPtr = (byte*)BinaryImage.DataPointer;
            int Width = BinaryImage.Width, Height = BinaryImage.Height;
            if (IsPortrait)
            {
                int HeightCount = 0;
                int Offset = 0;
                for (int y = 0; y < Height; ++y)
                {
                    int DetectedCount = 0;
                    for (int x = 0; x < Width; ++x)
                    {
                        if (ImgPtr[Offset++] > 0)
                            DetectedCount++;
                    }
                    if (DetectedCount >= WidthRequest)
                        ++HeightCount;
                }
                return HeightCount / (double)Height;
            }else
            {   //Landscape
                int[] Counter = new int[Width];
                int Offset = 0;
                for (int y = 0; y < Height; ++y)
                {
                    for (int x = 0; x < Width; ++x)
                    {
                        if (ImgPtr[Offset++] > 0)
                            Counter[x]++;
                    }
                }
                int WidthCount = 0;
                for (int x = 0; x < Width; ++x)
                    if (Counter[x] >= WidthRequest)
                        WidthCount++;
                return WidthCount / (double)Width;
            }
        }
        private static unsafe bool IsPixelArgbColorSame(in byte* Src1, in byte* Src2, in int errors)
        {
            int[] ColorDiff =
            {
                Math.Abs(Src1[0] - Src2[0]),    //B
                Math.Abs(Src1[1] - Src2[1]),    //G
                Math.Abs(Src1[2] - Src2[2]),    //R
                Math.Abs(Src1[3] - Src2[3]),    //A
            };
            for (int i = 0; i < 4; ++i)
                if (ColorDiff[i] > errors)
                    return false;
            return true;
        }
        private static unsafe int FindSimilarColorInList(in byte* Pixel, in int[] ColorList, in int[] ColorMaskList, in int errors)
        {
            //Compare Suitable Color
            for (int i = 0; i < ColorList.Length; ++i)
            {
                int ColorMask = (ColorMaskList != null && i < ColorMaskList.Length) ? ColorMaskList[i] : ~0;
                int ColorArgb = ColorList[i] & ColorMask;
                int PixelColor = (*((int*)Pixel)) & ColorMask;
                if (IsPixelArgbColorSame((byte*)&PixelColor, (byte*)&ColorArgb, errors))
                    return i;
            }
            return -1;
        }
        private static unsafe int IsColorMixedByColor(byte *Pixel, in int[] ColorList, in byte* Color)
        {
            float[] Fraction = new float[3];
            int[] ColorDelta = new int[3];
            for (int c = 0; c < 3; c++)
            {
                ColorDelta[c] = ((int)Pixel[c] - Color[c]);
            }
            for (int i = 0; i < ColorList.Length; ++i)
            {
                fixed (int* ColorArgbPtr = &ColorList[i])
                {
                    byte* ColorArgbBytePtr = (byte*)ColorArgbPtr;
                    for (int c = 0; c < 3; c++)
                    {
                        Fraction[c] = ColorDelta[c] / (float)((int)ColorArgbPtr[c] - (int)Color[c]);
                    }
                    if (Math.Abs(Fraction[0] - Fraction[1]) < 0.02 && Math.Abs(Fraction[2] - Fraction[1]) < 0.02 && Math.Abs(Fraction[0] - Fraction[2]) < 0.02)
                        return i;
                }
            }
            return -1;
        }
        private static void ElimateBackgroundWithSolidColor(in Mat InputImage, ref Mat OutputImage, in Color[] ItemColor, in int[] ColorMaskList)
        {
            if (!InputImage.Size.Equals(OutputImage.Size))
            {
                OutputImage.Dispose();
                OutputImage = new Mat(OutputImage.Size, DepthType.Cv8U, 1);
            }

            int[] ItemColorArgb;
            if (ItemColor != null && ItemColor.Length > 0)
                ItemColorArgb = ItemColor.Select(c => c.ToArgb()).ToArray();
            else
                throw new Exception("No Item Color Infomation");

            unsafe
            {
                byte* InputImageColor = (byte*)InputImage.DataPointer;
                byte* OutputImageColor = (byte*)OutputImage.DataPointer;
                int Offset = 0;
                for (int y = 0; y < InputImage.Height; ++y)
                {
                    for (int x = 0; x < InputImage.Width; ++x)
                    {
                        int SimilarItemColorID = FindSimilarColorInList(&InputImageColor[Offset * 4], ItemColorArgb, ColorMaskList, 6);
                        if (SimilarItemColorID >= 0)
                            OutputImageColor[Offset] = 255; //Item Set as White
                        else
                            OutputImageColor[Offset] = 0; //Background Set as Black
                        Offset++;
                    }
                }
            }
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
                //CvInvoke.Imwrite("O:\\Ori.png", Data);

                ElimateBackgroundWithSolidColor(in Data, ref BackgroundRemovalImage, new Color[] { Color.White, Color.Red }, new int[] { ~0, 0xFF << 16 });
                Console.WriteLine(BarLengthCalc(BackgroundRemovalImage, 4, true));
                //CvInvoke.Imwrite("O:\\BackgroundElimation.png", BackgroundRemovalImage);
                IsProcessingData = false;
                continue;
                //CvInvoke.Imwrite("O:\\Out.png", BackgroundRemovalImage);
                IsProcessingData = false;
                unsafe
                {
                    byte* InputImageData = (byte*)Data.DataPointer;
                    byte* OutputImageData = (byte*)BackgroundRemovalImage.DataPointer;
                    for (int i = 0, height = Data.Size.Height, offset = 0; i < height; ++i)
                    {
                        for (int j = 0, width = Data.Size.Width; j < width; ++j)
                        {
                            if (InputImageData[offset + 0] > 200 &&
                                InputImageData[offset + 1] > 200 &&
                                InputImageData[offset + 2] > 200)
                                OutputImageData[offset >> 2] = 255;
                            offset += 4;
                        }
                    }
                }
            }
        }
    }
}
