using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageProcessModule
{
    public partial class ImageProcessBase
    {
        /// <summary>
        /// The fraction of left border in this Image.
        /// 0 is the most left. 1 is the most right
        /// </summary>
        protected virtual double Clipped_Left
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// The fraction of top border in this Image.
        /// 0 is the most top. 1 is the most bottom
        /// </summary>
        protected virtual double Clipped_Top
        {
            get
            {
                return 0;
            }
        }
        /// <summary>
        /// The fraction of right border in this Image.
        /// 0 is the most left. 1 is the most right.
        /// </summary>
        protected virtual double Clipped_Right
        {
            get
            {
                return 1;
            }
        }
        /// <summary>
        /// The fraction of bottom border in this Image.
        /// 0 is the most top. 1 is the most bottom
        /// </summary>
        protected virtual double Clipped_Bottom
        {
            get
            {
                return 1;
            }
        }
        /// <summary>
        /// The scale of this image.
        /// </summary>
        protected virtual ImageScaleType ImageScale
        {
            get
            {
                return ImageScaleType.OriginalSize;
            }
        }
        /// <summary>
        /// Does the image need to be cliped
        /// </summary>
        private bool IsImageClip
        {
            get
            {
                return !(Clipped_Left == 0 && Clipped_Right == 1 && Clipped_Top == 0 && Clipped_Bottom == 1);
            }
        }
        /// <summary>
        /// A flag for multi-threading.
        /// If the worker thread is still processing data, the value should be true.
        /// When IsProcessingData is true, the dispatcher will not assign new image to this class.
        /// </summary>
        protected volatile bool IsProcessingData = false;
        /// <summary>
        /// A flag for multi-threading.
        /// If a new image is accepting, the flag will be true.
        /// When IsUpdatingData is true, the worker thread should wait for the new data.
        /// </summary>
        private volatile bool IsUpdatingData = false;
        /// <summary>
        /// The processed image will be store in Data.
        /// </summary>
        protected Mat Data = new Mat();
        /// <summary>
        /// A new thread will run this ImageHandler if the parameter of constructor 'IsCreateNewThread' is true.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void ImageHandler(object args)
        {

        }
        /// <summary>
        /// Constructor of ImageProcessBase.
        /// New thread will be created when construction.
        /// </summary>
        /// <param name="IsAddImageProcessList">The object will be added to a list that stores all objects whose class is devided from ImageProcessBase. Only when this is true, the object will continuously receive new frame.</param>
        /// <param name="IsCreateNewThread">A new thread will be created for computing. If it is false, then it is necessary to call ImageHandler.</param>
        public ImageProcessBase(bool IsAddImageProcessList = true, bool IsCreateNewThread = true)
        {
            if (IsAddImageProcessList)
            {
                imageProcesses.Add(this);
                IsImageScaleUsed[(int)ImageScale] = true;
            }
            if (IsCreateNewThread)
                ThreadPool.QueueUserWorkItem(new WaitCallback(ImageHandler));
        }
        public unsafe void TryUpdateData(in Mat ResizedData)
        {
            if (IsProcessingData)   //If the worker thread is still computing
                return;
            else if (IsUpdatingData)    //If it is updating image
                return;
            else
                IsUpdatingData = true;  //Set the data is updating

            if (!IsImageClip)   //Does image need to be clipped
            {
                if (Data.Rows != ResizedData.Rows || Data.Cols != ResizedData.Cols) //Check output size is correct
                {
                    Data?.Dispose();    //Release old Data memory
                    Data = new Mat(ResizedData.Rows, ResizedData.Cols, DepthType.Cv8U, 4);
                }
                long TotalSize = ResizedData.Rows * ResizedData.Cols * 4;   //Copy the total size directly
                Buffer.MemoryCopy(ResizedData.DataPointer.ToPointer(), Data.DataPointer.ToPointer(), TotalSize, TotalSize);
            }
            else
            {   
                //The image need to be clipped
                int ImageLeft = (int)(ResizedData.Cols * Clipped_Left),
                    ImageRight = (int)(ResizedData.Cols * Clipped_Right),
                    ImageTop = (int)(ResizedData.Rows * Clipped_Top),
                    ImageBottom = (int)(ResizedData.Rows * Clipped_Bottom);
                int Data_Width = ImageRight - ImageLeft,
                    Data_Height = ImageBottom - ImageTop;

                if (Data.Rows != Data_Height || Data.Cols != Data_Width)    //Check output size is correct
                {
                    Data?.Dispose();    //Release old Data memory
                    Data = new Mat(Data_Height, Data_Width, DepthType.Cv8U, 4);
                }
                int RawDataOffset =
                    +4 * ImageTop * ResizedData.Cols  //Skip Top Pixel
                    + 4 * ImageLeft;    //Skip Left Pixel
                int Length = 4 * Data_Width;    //Copy the size of new width per row

                for (int i = 0; i < Data_Height; ++i)
                {
                    int OutputDataOffset = (4 * i * Data_Width);    //4 Bytes * Data_Width * ith rows
                    IntPtr DstPointer = Data.DataPointer + OutputDataOffset,
                        SrcPointer = ResizedData.DataPointer + RawDataOffset;
                    Buffer.MemoryCopy(SrcPointer.ToPointer(), DstPointer.ToPointer(), Length, Length);
                    RawDataOffset += 4 * ResizedData.Cols;
                }
            }
            IsUpdatingData = false; //Update done flage
            IsProcessingData = true;    //Tell worker thread that it can be process now
        }
    }
}
