using Accord.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.CvEnum;
using ImageProcessModule;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace ScreenCapture
{
    public class CardCapture : CaptureMethod
    {
        /*Const Variable*/
        const int PreCreateMatCount = 30;
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
        /// <param name="bitmapBuffer">Communication pipe with other threads. It stores some processing Mat and some unused Mat</param>
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

            //Store Mat Buffer
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
                //Create enough UnusedMat
                for (int i = 0; i < PreCreateMatCount; ++i)
                    bitmapBuffer.PushUnusedMat(CreateSuitableMat());
                Thread.CurrentThread.Priority = ThreadPriority.Highest; //Set ThreadPriority of Accord.Video.DirectShow
            }

            if (!IsStartCapturing)  //Is Start Capturing
                return;

            Bitmap SrcBitmap = eventArgs.Frame; //Get Frame from Capture Card
            Mat ProcessingMat = bitmapBuffer.GetUnusedMat();   //Get Mat from Buffer

            if (ProcessingMat == null)
                ProcessingMat = CreateSuitableMat();

            //Unlock Bitmap & Get actual pointer
            BitmapData SrcBitmapData = SrcBitmap.LockBits(new Rectangle(new Point(0, 0), SrcBitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            IntPtr SrcBitmapDataPointer = SrcBitmapData.Scan0;
            IntPtr DstBitmapDataPointer = ProcessingMat.DataPointer;

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
            SrcBitmap.UnlockBits(SrcBitmapData);
            //Push to buffer
            bitmapBuffer.PushProcessingMat(ProcessingMat);
        }
        /// <summary>
        /// Pre-create Mat for image processing.
        /// Pre-create helps allocate continuous memory address, which is cache-friendly.
        /// </summary> 
        private Mat CreateSuitableMat()
        {
            return new Mat(height, width, DepthType.Cv8U, 4);
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