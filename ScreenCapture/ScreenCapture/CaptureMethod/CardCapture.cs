using Accord.Video.DirectShow;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace ScreenCapture
{
    public class CardCapture : CaptureMethod
    {
        /*Const Variable*/
        const int PreCreateBitmapCount = 30;
        /*Const Variable*/

        /*Screen Capture Variable*/
        FilterInfoCollection videoDevices;
        VideoCaptureDevice device;
        int width = 0, height = 0;
        /*Screen Capture Variable*/

        /*Multi-Thread Variable*/
        BitmapBuffer bitmapBuffer;
        /*Multi-Thread Variable*/

        public CardCapture(BitmapBuffer bitmapBuffer, int DeviceID = 0)
        {
            //Search all video devices
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (DeviceID < videoDevices.Count)
                device = new VideoCaptureDevice(videoDevices[DeviceID].MonikerString, PixelFormat.Format32bppArgb);
            else
                throw new ArgumentException("No this device exists!");
            device.NewFrame += NewFrameArrived;
            device.Start();
            CacheOptimizer.ResetAllAffinity();

            this.bitmapBuffer = bitmapBuffer;
        }

        private void NewFrameArrived(object sender, Accord.Video.NewFrameEventArgs eventArgs)
        {
            if (width == 0)
            {
                width = eventArgs.Frame.Width;
                height = eventArgs.Frame.Height;
                //Create enough UnusedBitmap
                for (int i = 0; i < PreCreateBitmapCount; ++i)
                    bitmapBuffer.PushUnusedBitmap(CreateSuitableBitmap());
            }
            Bitmap SrcBitmap = eventArgs.Frame;
            Bitmap ProcessingBitmap = bitmapBuffer.GetUnusedBitmap();
            BitmapData SrcBitmapData = SrcBitmap.LockBits(new Rectangle(new Point(0, 0), SrcBitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData ProcessingBitmapData = ProcessingBitmap.LockBits(new Rectangle(new Point(0, 0), ProcessingBitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            IntPtr SrcBitmapDataPointer = SrcBitmapData.Scan0;
            IntPtr DstBitmapDataPointer = ProcessingBitmapData.Scan0;
            if (SrcBitmapData.Stride != ProcessingBitmapData.Stride)
                throw new Exception("Error! Stride Different!");
            int TotalLength = height * ProcessingBitmapData.Stride * 4;
            unsafe
            {
                Buffer.MemoryCopy(SrcBitmapDataPointer.ToPointer(), DstBitmapDataPointer.ToPointer(), TotalLength, TotalLength);
            }
            ProcessingBitmap.UnlockBits(ProcessingBitmapData);
            SrcBitmap.UnlockBits(SrcBitmapData);
            bitmapBuffer.PushProcessingBitmap(ProcessingBitmap);
        }

        private Bitmap CreateSuitableBitmap()
        {
            return new Bitmap(width, height, PixelFormat.Format32bppArgb);
        }
        public void Start()
        {
            if (!device.IsRunning)
                device.Start();
        }
        public void Stop()
        {
            device.Stop();
        }

    }
}