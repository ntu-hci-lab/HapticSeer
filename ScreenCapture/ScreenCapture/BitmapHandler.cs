using SharpDX.IO;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace CaptureSampleCore
{
    class BitmapInfo
    {
        public ulong timestamp;
        public Bitmap bitmap;
        public BitmapInfo(Bitmap bitmap, ulong timestamp)
        {
            this.bitmap = bitmap;
            this.timestamp = timestamp;
        }
    }

    public class BitmapHandler
    {
        private BufferBlock<BitmapInfo> bufferBlock = new BufferBlock<BitmapInfo>();
        private string OutputPath;
        public bool IsStart = true;
        ~BitmapHandler()
        {
            bufferBlock.Complete();
        }
        public void PushBuffer(Bitmap bitmap, ulong timestamp)
        {
            BitmapInfo bitmapInfo = new BitmapInfo(bitmap, timestamp);
            bufferBlock.Post(bitmapInfo);
        }
        static async Task ConsumeData(Object bitmapHandlerObj)
        {
            BitmapHandler bitmapHandler = bitmapHandlerObj as BitmapHandler;
            ISourceBlock<BitmapInfo> source = bitmapHandler.bufferBlock;
            byte[] pixelData = null;
            while (await source.OutputAvailableAsync().ConfigureAwait(true))
            {
                if (bitmapHandler.IsStart)
                {
                    BitmapInfo data = source.Receive();
                    Bitmap bitmap = data.bitmap;
                    ulong timestamp = data.timestamp;
                    int width = bitmap.Size.Width, height = bitmap.Size.Height;
                    if (pixelData == null || pixelData?.Length != width * height * 4)
                        pixelData = new byte[width * height * 4];
                    System.Drawing.Bitmap sysBitmap = new System.Drawing.Bitmap(width, height);
                    bitmap.CopyPixels(pixelData, width * 4);
                    var bd = sysBitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    Marshal.Copy(pixelData, 0, bd.Scan0, pixelData.Length);
                    sysBitmap.UnlockBits(bd);
                    bitmap.Dispose();
                    /*
                    var factory = new ImagingFactory();
                    using (var wic = new WICStream(factory, bitmapHandler.OutputPath + timestamp.ToString() + ".jpeg", NativeFileAccess.ReadWrite))
                    using (var encoder = new JpegBitmapEncoder(factory, wic))
                    using (var frame = new BitmapFrameEncode(encoder))
                    {
                        frame.Initialize();
                        frame.SetSize(bitmap.Size.Width, bitmap.Size.Height);
                        Guid format = bitmap.PixelFormat;
                        frame.SetPixelFormat(ref format);
                        frame.WriteSource(bitmap);
                        frame.Commit();
                        encoder.Commit();
                    }
                    factory.Dispose();
                    bitmap.Dispose();
                    */
                }
            }
        }
        public BitmapHandler(string OutputPath, int ThreadNums = 2)
        {
            this.OutputPath = OutputPath;
            for (int i = 0; i < ThreadNums; ++i)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback((s)=> 
                {
                    var consumer = ConsumeData(s);
                    consumer.Wait();
                }), this);
            }
        }
    }
}
