using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessModule
{
    public partial class ImageProcessBase
    {
        public static Mat Kernel_4x4
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
        public static Mat _Kernel_4x4 = null;
        public static Mat Kernel_2x2
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
        public static Mat _Kernel_2x2 = null;
        public enum ImageScaleType
        {
            OriginalSize,
            Half,
            Quarter
        }
        private static bool[] IsImageScaleUsed = new bool[Enum.GetNames(typeof(ImageScaleType)).Length];
        /// <summary>
        /// A list that store all class variable whose class is devided from ImageProcessBase.
        /// </summary>
        private static List<ImageProcessBase> imageProcesses = new List<ImageProcessBase>();
        /// <summary>
        /// Variables that store the resized images
        /// </summary>
        private static Mat ResizedHalfData, ResizedQuarterData;
        /// <summary>
        /// Call TryUpdateAllData to pass new frame to all class derived from ImageProcessBase 
        /// </summary>
        /// <param name="rawData">New frame</param>
        public static void TryUpdateAllData(in Mat rawData)
        {
            // Enumerate all possible scale
            foreach (ImageScaleType imageScale in Enum.GetValues(typeof(ImageScaleType)))
            {
                // Check is the scale used 
                if (IsImageScaleUsed[(int)imageScale] != true)
                    continue;   // Not used
                // Check scale
                switch (imageScale)
                {
                    case ImageScaleType.Half:
                        if (ResizedHalfData == null)    // If not created yet, create new one
                            ResizedHalfData = new Mat(rawData.Rows / 2, rawData.Cols / 2, DepthType.Cv8U, 4);
                        CvInvoke.Resize(rawData, ResizedHalfData, ResizedHalfData.Size);
                        break;
                    case ImageScaleType.Quarter:
                        if (ResizedQuarterData == null)    // If not created yet, create new one
                            ResizedQuarterData = new Mat(rawData.Rows / 4, rawData.Cols / 4, DepthType.Cv8U, 4); 
                        CvInvoke.Resize(rawData, ResizedQuarterData, ResizedQuarterData.Size);
                        break;
                }
            }
            // Enumerate all objects devided from ImageProcessBase
            foreach (ImageProcessBase imgProc in imageProcesses)
            {
                // Check scale & send the corresponding scale image 
                switch (imgProc.ImageScale)
                {
                    case ImageScaleType.OriginalSize:
                        imgProc.TryUpdateData(in rawData);
                        break;
                    case ImageScaleType.Half:
                        imgProc.TryUpdateData(in ResizedHalfData);
                        break;
                    case ImageScaleType.Quarter:
                        imgProc.TryUpdateData(in ResizedQuarterData);
                        break;
                }
            }
        }

        public enum ElimateColorApproach
        {
            RemoveSimilarColor_ReserveDifferentColor,
            ReserveSimilarColor_RemoveDifferentColor
        }
        /// <summary>
        /// Only keeps/removes specific colors in Input Image (with Bitwise Mask)
        /// </summary>
        /// <param name="InputImage">Source Image</param>
        /// <param name="OutputImage">Output Target. One channel image.</param>
        /// <param name="ItemColor">A list that stores all colors that should be reserve.</param>
        /// <param name="ColorMaskList">A list that stores all masks for color. For example, 0x00FFFFFF means ignore Alpha channel.</param>
        /// <param name="RemoveApproach">The approach to remove color.</param>
        /// <param name="ColorErrorsInOneChannel">The tolerance of color in one channel.</param>
        public static void ElimateBackgroundWithSearchingSimilarColor(in Mat InputImage, ref Mat OutputImage, in Color[] ItemColor, in uint[] ColorMaskList, ElimateColorApproach RemoveApproach, int ColorErrorsInOneChannel = 6)
        {
            /*Assertion*/
            Trace.Assert(ItemColor != null, "ItemColor is null!");
            Trace.Assert(ColorMaskList != null, "ColorMaskList is null!");
            Trace.Assert(ItemColor.Length == ColorMaskList.Length, "Length of ItemColor is not equal to mask count in ColorMaskList!");
            /*Assertion*/

            if (!InputImage.Size.Equals(OutputImage.Size))
            {
                OutputImage.Dispose();
                OutputImage = new Mat(OutputImage.Size, DepthType.Cv8U, 1);
            }

            int[] ItemColorArgb = ItemColor.Select(c => c.ToArgb()).ToArray();
            bool IsRemoveSimilarColor = RemoveApproach.Equals(ElimateColorApproach.RemoveSimilarColor_ReserveDifferentColor);
            unsafe
            {
                byte* InputImageColor = (byte*)InputImage.DataPointer;
                byte* OutputImageColor = (byte*)OutputImage.DataPointer;
                int Offset = 0;
                // Scan the whole image
                for (int y = 0; y < InputImage.Height; ++y)
                {
                    for (int x = 0; x < InputImage.Width; ++x)
                    {
                        // Is there any similar color in wanna-reserve color list
                        int SimilarItemColorID = FindSimilarColorInList(&InputImageColor[Offset * 4], ItemColorArgb, ColorMaskList, ColorErrorsInOneChannel);
                        bool IsColorSimilar = SimilarItemColorID >= 0;

                        if (IsColorSimilar && IsRemoveSimilarColor || !IsColorSimilar && !IsRemoveSimilarColor)
                            OutputImageColor[Offset] = 0; // Background Set as Black
                        else
                            OutputImageColor[Offset] = 255; // Item Set as White

                        Offset++;
                    }
                }
            }
        }
        /// <summary>
        /// Check are the two color the same with a tolerance value.
        /// </summary>
        /// <param name="Src1">Pointer of Color 1</param>
        /// <param name="Src2">Pointer of Color 2</param>
        /// <param name="ColorErrorsInOneChannel">The tolerance of color in one channel.</param>
        /// <returns></returns>
        public static unsafe bool IsPixelArgbColorSame(in byte* Src1, in byte* Src2, in int ColorErrorsInOneChannel)
        {
            /*Assertion*/
            Trace.Assert(Src1 != null, "Src1 is null!");
            Trace.Assert(Src2 != null, "Src2 is null!");
            /*Assertion*/

            // Compute Color Difference in each channel
            int[] ColorDiff =
            {
                Math.Abs(Src1[0] - Src2[0]),    //B
                Math.Abs(Src1[1] - Src2[1]),    //G
                Math.Abs(Src1[2] - Src2[2]),    //R
                Math.Abs(Src1[3] - Src2[3]),    //A
            };

            // Check errors in each channel 
            foreach (int OneChannelDiff in ColorDiff)
                // If any channel has an error bigger than tolerance
                if (OneChannelDiff > ColorErrorsInOneChannel)
                    return false;
            return true;
        }
        /// <summary>
        /// Check is there any similar color in ColorList.
        /// </summary>
        /// <param name="Pixel">Compared Color</param>
        /// <param name="ColorList">Candidate Color</param>
        /// <param name="ColorMaskList">Mask of Candidate Color</param>
        /// <param name="errors">The tolerance of color in one channel</param>
        /// <returns>The index of similar color in ColorList. Return -1 if no similar color.</returns>
        private static unsafe int FindSimilarColorInList(in byte* Pixel, in int[] ColorList, in uint[] ColorMaskList, in int ColorErrorsInOneChannel)
        {
            /*Assertion*/
            Trace.Assert(ColorList != null, "ItemColor is null!");
            Trace.Assert(ColorMaskList != null, "ColorMaskList is null!");
            Trace.Assert(ColorList.Length == ColorMaskList.Length, "Length of ColorList is not equal to mask count in ColorMaskList!");
            /*Assertion*/

            // Get Color Argb
            int PixelColorArgb = *(int*)Pixel;
            //Compare Suitable Color
            for (int i = 0; i < ColorList.Length; ++i)
            {
                // Get Color Mask
                uint ColorMask = ColorMaskList[i];
                // Do Mask Compute
                int MaskedColorArgb = (int)(ColorList[i] & ColorMask);
                int MaskedPixelColorArgb = (int)(PixelColorArgb & ColorMask);
                // Compare Color
                if (IsPixelArgbColorSame((byte*)&MaskedColorArgb, (byte*)&MaskedPixelColorArgb, ColorErrorsInOneChannel))
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// Get the length of bar (binary) image
        /// </summary>
        /// <param name="BinaryImage">One channel image.</param>
        /// <param name="WidthRequest">The threshold of bar width</param>
        /// <param name="IsPortrait">The attitude of bar.</param>
        /// <returns>The friction of bar.</returns>
        public static unsafe double BarLengthCalc(in Mat BinaryImage, in int WidthRequest, in bool IsPortrait)
        {
            // Get the raw data of Binary Image
            byte* ImgPtr = (byte*)BinaryImage.DataPointer;
            int Width = BinaryImage.Width, Height = BinaryImage.Height;

            if (IsPortrait)
            {
                int HeightCount = 0;
                int Offset = 0;
                for (int y = 0; y < Height; ++y)
                {
                    int DetectedCount = 0;
                    // Count all white pixel in this row
                    for (int x = 0; x < Width; ++x)
                        if (ImgPtr[Offset++] > 0)
                            DetectedCount++;
                    // Compare to the width threshold
                    if (DetectedCount >= WidthRequest)
                        ++HeightCount;
                }
                return HeightCount / (double)Height;
            }
            else
            {   // Landscape
                // Store the count of white pixel in the same col
                int[] Counter = new int[Width];
                int Offset = 0;
                
                // Search all pixels in Img
                for (int y = 0; y < Height; ++y)
                    for (int x = 0; x < Width; ++x)
                        if (ImgPtr[Offset++] > 0)
                            Counter[x]++;

                int WidthCount = 0;
                // Check is the count of white pixel exceed the threshold
                for (int x = 0; x < Width; ++x)
                    if (Counter[x] >= WidthRequest)
                        WidthCount++;
                return WidthCount / (double)Width;
            }
        }
        /// <summary>
        /// Get the angle of a point from the center of screen
        /// </summary>
        /// <param name="x">Position x of the point</param>
        /// <param name="y">Position y of the point</param>
        /// <param name="Width">Screen Width</param>
        /// <param name="Height">Screen Height</param>
        /// <returns>Angle in deg</returns>
        public static double GetAngleByCircleModel(double x, double y, double Width, double Height)
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
