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
        bool IsStartCapturing = false;
        /*Multi-Thread Variable*/
        /// <summary>
        /// Create CardCapture to communicate with Capture Card (by DirectShow.)
        /// </summary>
        /// <param name="bitmapBuffer">Communication pipe with other threads. It stores some processing Bitmap and some unused Bitmap</param>
        /// <param name="DeviceID">The DeviceID of Capture Card.</param>
        public CardCapture(BitmapBuffer bitmapBuffer, int DeviceID = 0)
        {
            //Search all video devices
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //Check DeviceID
            if (DeviceID < videoDevices.Count)
                device = new VideoCaptureDevice(videoDevices[DeviceID].MonikerString, PixelFormat.Format24bppRgb);
            else
                throw new ArgumentException("No this device exists!");
            //Register the callback function when frame is arrived
            device.NewFrame += NewFrameArrived;
            //Start the DirectShow
            device.Start();
            //Cache Optimized for Ryzen-based 16 Core CPU
            CacheOptimizer.ResetAllAffinity();

            //Store Bitmap Buffer
            this.bitmapBuffer = bitmapBuffer;
        }

        /// <summary>
        /// Called from Accord.Video.DirectShow
        /// Thread is created by Accord.Video.DirectShow.
        /// The thread also communicates with Capture Card.
        /// The performance here will impact the frame rate of capturing.
        /// </summary>
        private void NewFrameArrived(object sender, Accord.Video.NewFrameEventArgs eventArgs)
        {
            if (width == 0) //Is initialized
            {
                width = eventArgs.Frame.Width;
                height = eventArgs.Frame.Height;
                //Create enough UnusedBitmap
                for (int i = 0; i < PreCreateBitmapCount; ++i)
                    bitmapBuffer.PushUnusedBitmap(CreateSuitableBitmap());
                Thread.CurrentThread.Priority = ThreadPriority.Highest; //Set ThreadPriority of Accord.Video.DirectShow
            }

            if (!IsStartCapturing)  //Is Start Capturing
                return;

            Bitmap SrcBitmap = eventArgs.Frame; //Get Frame from Capture Card
            Bitmap ProcessingBitmap = bitmapBuffer.GetUnusedBitmap();   //Get Bitmap from Buffer
            
            //Unlock Bitmap & Get actual pointer
            BitmapData SrcBitmapData = SrcBitmap.LockBits(new Rectangle(new Point(0, 0), SrcBitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData ProcessingBitmapData = ProcessingBitmap.LockBits(new Rectangle(new Point(0, 0), ProcessingBitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            IntPtr SrcBitmapDataPointer = SrcBitmapData.Scan0;
            IntPtr DstBitmapDataPointer = ProcessingBitmapData.Scan0;

            if ((SrcBitmapData.Stride % 4) != 0)    //Source Image should align 32-bit
                throw new Exception("Error! Source Image Not Align 32-bit!");

            int TotalLength = height * width;

            //Fast 24-bit -> 32-bit for High-Throughput Read/Write/Shuffle CPU Architecture
            //According to https://gmplib.org/~tege/x86-timing.pdf
            //Throughput of shl: 4
            unsafe
            {
                uint* src = (uint*)SrcBitmapDataPointer;
                uint* dst = (uint*)DstBitmapDataPointer;
                for (int i = 0; i < TotalLength; i += 4)
                {
                    uint sa = src[0];
                    uint sb = src[1];
                    uint sc = src[2];

                    dst[i + 0] = sa;
                    dst[i + 1] = (sa >> 24) | (sb << 8);
                    dst[i + 2] = (sb >> 16) | (sc << 16);
                    dst[i + 3] = sc >> 8;

                    src += 3;
                }
            }
            //Release Locks
            ProcessingBitmap.UnlockBits(ProcessingBitmapData);
            SrcBitmap.UnlockBits(SrcBitmapData);
            //Push to buffer
            bitmapBuffer.PushProcessingBitmap(ProcessingBitmap);
        }
        /// <summary>
        /// Pre-create Bitmap for image processing.
        /// Pre-create helps allocate continuous memory address, which is cache-friendly.
        /// </summary> 
        private Bitmap CreateSuitableBitmap()
        {
            return new Bitmap(width, height, PixelFormat.Format32bppRgb);   
        }
        /// <summary>
        /// Call Start to activate image capture.
        /// </summary>
        public void Start()
        {
            if (!device.IsRunning)
                device.Start();
            IsStartCapturing = true;
        }
        /// <summary>
        /// Call Stop to deactivate image capture.
        /// </summary>
        public void Stop()
        {
            device.Stop();
            IsStartCapturing = false;
        }

    }
}