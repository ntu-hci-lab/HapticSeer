//#define DEBUG_VR
#define DEBUG_RACE 
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
        private static unsafe void ElimateBackgroundWithSimilarItemColor(in Mat Ori, ref Mat Out, in Color[] ItemColor, in int errors)
        {
            unsafe
            {
                byte* InputImageData = (byte*)Ori.DataPointer;
                byte* OutputImageData = (byte*)Out.DataPointer;
                for (int i = 0, height = Ori.Size.Height, offset = 0; i < height; ++i)
                {
                    for (int j = 0, width = Ori.Size.Width; j < width; ++j)
                    {
                        OutputImageData[offset >> 2] = 0;
                        for (int k = 0; k < ItemColor.Length; ++k)
                        {
                            int ColorArgb = ItemColor[k].ToArgb();
                            int* ColorPtr = &ColorArgb;
                            if (IsPixelArgbColorSame(&InputImageData[offset], (byte*)ColorPtr, errors))
                                OutputImageData[offset >> 2] = 255;
                        }
                        offset += 4;
                    }
                }
            }
        }
        private int index = 0;
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
#if DEBUG_VR
                ElimateBackgroundWithSolidColor(in Data, ref BackgroundRemovalImage, new Color[] { Color.White, Color.Red }, new int[] { ~0, 0xFF << 16 });
                Console.WriteLine(BarLengthCalc(BackgroundRemovalImage, 4, true));
#endif
#if DEBUG_RACE
                ElimateBackgroundWithSimilarItemColor(in Data, ref BackgroundRemovalImage, new Color[] { Color.White }, 70);
#endif
                CvInvoke.Imwrite("O:\\" + index++ + ".png", BackgroundRemovalImage);
                IsProcessingData = false;
            }
        }
    }
}
