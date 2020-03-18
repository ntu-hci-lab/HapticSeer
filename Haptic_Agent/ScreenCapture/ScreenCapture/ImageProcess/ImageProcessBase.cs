using Emgu.CV;
using Emgu.CV.CvEnum;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFCaptureSample.ScreenCapture.ImageProcess
{
    class ImageProcessBase
    {
        protected static Mat Kernel_4x4
        {
            get
            {
                if (_Kernel_4x4 != null)
                    return _Kernel_4x4;
                _Kernel_4x4 = new Mat(4, 4, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                unsafe
                {
                    byte* KernelPtr = (byte*)_Kernel_4x4.DataPointer;
                    for (int i = 0; i < 16; ++i)
                        KernelPtr[i] = 255;
                }
                return Kernel_4x4;
            }
        }
        protected static Mat _Kernel_4x4 = null;
        protected static Mat Kernel_2x2
        {
            get
            {
                if (_Kernel_2x2 != null)
                    return _Kernel_2x2;
                _Kernel_2x2 = new Mat(2, 2, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                unsafe
                {
                    byte* KernelPtr = (byte*)_Kernel_2x2.DataPointer;
                    for (int i = 0; i < 4; ++i)
                        KernelPtr[i] = 255;
                }
                return _Kernel_2x2;
            }
        }
        protected static Mat _Kernel_2x2 = null;
        protected virtual double Clipped_Left
        {
            get
            {
                return 0;
            }
        }
        protected virtual double Clipped_Top
        {
            get
            {
                return 0;
            }
        }
        protected virtual double Clipped_Right
        {
            get
            {
                return 1;
            }
        }
        protected virtual double Clipped_Bottom
        {
            get
            {
                return 1;
            }
        }
        protected virtual double Scale_Width
        {
            get
            {
                return 1;
            }
        }
        protected virtual double Scale_Height
        {
            get
            {
                return 1;
            }
        }
        private bool IsImageClip
        {
            get
            {
                return !(Clipped_Left == 0 && Clipped_Right == 1 && Clipped_Top == 0 && Clipped_Bottom == 1);
            }
        }
        private bool IsImageScale
        {
            get
            {
                return !(Scale_Width == 1 && Scale_Height == 1);
            }
        }
        protected volatile bool IsProcessingData = false;
        private volatile bool IsUpdatingData = false;
        protected Mat Data = new Mat();
        private Mat ScaledRawData = new Mat();
        private static List<ImageProcessBase> imageProcesses = new List<ImageProcessBase>();
        protected virtual void ImageHandler(object args)
        {

        }
        public ImageProcessBase(bool IsAddImageProcessList = true, bool IsCreateNewThread = true)
        {
            if (IsAddImageProcessList)
                imageProcesses.Add(this);
            if (IsCreateNewThread)
                ThreadPool.QueueUserWorkItem(new WaitCallback(ImageHandler));
        }
        public static void TryUpdateAllData(Mat rawData)
        {
            //CvInvoke.Imwrite("O:\\Data.png", rawData);
            foreach (ImageProcessBase imgProc in imageProcesses)
                imgProc.TryUpdateData(rawData);
        }
        public void TryUpdateData(Mat rawData)
        {
            if (IsProcessingData)
                return;
            else if (IsUpdatingData)
                return;
            else
                IsUpdatingData = true;

            int ImageHeight, ImageWidth;
            if (!IsImageScale)
            {
                ImageHeight = rawData.Rows;
                ImageWidth = rawData.Cols;
                ScaledRawData = rawData;
            }else
            {
                ImageHeight = (int)(rawData.Rows * Scale_Height);
                ImageWidth = (int)(rawData.Cols * Scale_Width);
                if (ScaledRawData.Rows != ImageHeight || ScaledRawData.Cols != ImageWidth)
                {
                    ScaledRawData?.Dispose();
                    ScaledRawData = new Mat(ImageHeight, ImageWidth, DepthType.Cv8U, 4);
                }
                CvInvoke.Resize(rawData, ScaledRawData, ScaledRawData.Size);
            }

            if (!IsImageClip)
            {
                if (Data.Rows != ImageHeight || Data.Cols != ImageWidth)
                {
                    Data?.Dispose();
                    Data = new Mat(ImageHeight, ImageWidth, DepthType.Cv8U, 4);
                }
                Utilities.CopyMemory(Data.DataPointer, ScaledRawData.DataPointer, ImageHeight * ImageWidth * 4);
            }else
            {
                int ImageLeft = (int)(ImageWidth * Clipped_Left),
                    ImageRight = (int)(ImageWidth * Clipped_Right),
                    ImageTop = (int)(ImageHeight * Clipped_Top),
                    ImageBottom = (int)(ImageHeight * Clipped_Bottom);
                int Data_Width = ImageRight - ImageLeft,
                    Data_Height = ImageBottom - ImageTop;
                if (Data.Rows != Data_Height || Data.Cols != Data_Width)
                {
                    Data?.Dispose();
                    Data = new Mat(Data_Height, Data_Width, DepthType.Cv8U, 4);
                }
                int RawDataOffset = 
                    + 4 * ImageTop * ImageWidth  //Skip Top Pixel
                    + 4 * ImageLeft;    //Skip Left Pixel
                int Length = 4 * Data_Width;
                for (int i = 0; i < Data_Height; ++i)
                {
                    int OutputDataOffset = (4 * i * Data_Width);    //4 Bytes * Data_Width * ith rows
                    Utilities.CopyMemory(Data.DataPointer + OutputDataOffset, ScaledRawData.DataPointer + RawDataOffset, Length);
                    RawDataOffset += 4 * ImageWidth;
                }
            }

            IsUpdatingData = false;
            IsProcessingData = true;
        }
        public static void ElimateBackgroundWithSolidColor(in Mat InputImage, ref Mat OutputImage, in Color[] ItemColor, in uint[] ColorMaskList)
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
        public static unsafe bool IsPixelArgbColorSame(in byte* Src1, in byte* Src2, in int errors)
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
        private static unsafe int FindSimilarColorInList(in byte* Pixel, in int[] ColorList, in uint[] ColorMaskList, in int errors)
        {
            //Compare Suitable Color
            for (int i = 0; i < ColorList.Length; ++i)
            {
                uint ColorMask = (ColorMaskList != null && i < ColorMaskList.Length) ? ColorMaskList[i] : 0xFFFFFFFF;
                int ColorArgb = (int)(ColorList[i] & ColorMask);
                int PixelColor = (int)((*((int*)Pixel)) & ColorMask);
                if (IsPixelArgbColorSame((byte*)&PixelColor, (byte*)&ColorArgb, errors))
                    return i;
            }
            return -1;
        }
        public static unsafe int IsColorMixedByColor(byte* Pixel, in int[] ColorList, in byte* Color)
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

        public static unsafe void ElimateBackgroundWithSimilarItemColor(in Mat Ori, ref Mat Out, in Color[] ItemColor, in int errors)
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
        public static unsafe double BarLengthCalc(in Mat BinaryImage, in int WidthRequest, in bool IsPortrait)
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
            }
            else
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

        public double GetAngleByCircleModel(double x, double y, double Width, double Height)
        {
            x -= Width / 2;
            y -= Height / 2;
            y *= -1; //Coordinate Changes
            double Angle = Math.Atan2(y, x) / Math.PI * 180;
            if (Angle < 0)
                Angle += 360;
            return Angle;
        }
    }
}
