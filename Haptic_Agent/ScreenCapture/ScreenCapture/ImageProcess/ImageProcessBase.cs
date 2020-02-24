using Emgu.CV;
using Emgu.CV.CvEnum;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFCaptureSample.ScreenCapture.ImageProcess
{
    class ImageProcessBase
    {
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
            CvInvoke.Imwrite("O:\\TT.png", rawData);
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
    }
}
