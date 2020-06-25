﻿using Accord.Video.FFMPEG;
using SharpAvi;
using SharpAvi.Output;
using SharpDX.IO;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public System.Drawing.Bitmap bitmap;
        public BitmapInfo(SharpDX.WIC.Bitmap bitmap, ulong timestamp)
        {
            int width = bitmap.Size.Width, height = bitmap.Size.Height;
            byte[] pixelData = new byte[width * height * 4];
            System.Drawing.Bitmap sysBitmap = new System.Drawing.Bitmap(width, height);
            bitmap.CopyPixels(pixelData, width * 4); 
            var bd = sysBitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Marshal.Copy(pixelData, 0, bd.Scan0, pixelData.Length);
            sysBitmap.UnlockBits(bd);
            bitmap.Dispose();
            this.bitmap = sysBitmap;
            this.timestamp = timestamp;
        }
    }

    public class BitmapHandler
    {
        private BufferBlock<BitmapInfo> bufferBlock = new BufferBlock<BitmapInfo>();
        private string OutputPath;
        public bool IsStart = true;
        BasicCapture basicCapture; 
        VideoFileWriter writer = null;
        const int bps_per_frame = 512 * 1024;   //5kbps per frame
        
        ~BitmapHandler()
        {
            Done();
        }
        public async void Done()
        {
            while (bufferBlock.Count != 0)
                await Task.Delay(1);
            if (bufferBlock.Count == 0)
                bufferBlock.Complete();
        }
        public void PushBuffer(SharpDX.WIC.Bitmap bitmap, ulong timestamp)
        {
            BitmapInfo bitmapInfo = new BitmapInfo(bitmap, timestamp);
            bufferBlock.Post(bitmapInfo);
        }
        static async Task ConsumeData(Object bitmapHandlerObj)
        {
            BitmapHandler bitmapHandler = bitmapHandlerObj as BitmapHandler;
            ISourceBlock<BitmapInfo> source = bitmapHandler.bufferBlock;
            System.Drawing.Bitmap oldBitmap = null;
            int VideoFrameRate = 0;
            int NextFrameInsertPosition = 0;
            while (await source.OutputAvailableAsync().ConfigureAwait(false))
            {
                if (bitmapHandler.IsStart)
                {
                    BitmapInfo data = source.Receive();
                    System.Drawing.Bitmap bitmap = data.bitmap;
                    ulong timestamp = data.timestamp;
                    data.bitmap = null;
                    if (bitmapHandler.writer == null)
                    {
                        VideoFrameRate = (int)Math.Round(bitmapHandler.basicCapture.RecordFrameRate);
                        bitmapHandler.writer = new VideoFileWriter();
                        bitmapHandler.writer.Open(bitmapHandler.OutputPath + "Video.mp4", bitmap.Width, bitmap.Height, VideoFrameRate, Accord.Video.FFMPEG.VideoCodec.MPEG4, VideoFrameRate * bps_per_frame);
                        oldBitmap = bitmap;
                    }
                    int ThisFramePosition = (int)Math.Round((long)timestamp * VideoFrameRate / 1000f);
                    if (ThisFramePosition < NextFrameInsertPosition)    //Cannot put at corresponding position
                        ThisFramePosition++;   //Try to shift one frame
                    try
                    {
                        while (NextFrameInsertPosition < ThisFramePosition)
                        {
                            NextFrameInsertPosition++;
                            bitmapHandler.writer.WriteVideoFrame(oldBitmap);
                        }
                        if (NextFrameInsertPosition == ThisFramePosition)
                        {
                            NextFrameInsertPosition++;
                            bitmapHandler.writer.WriteVideoFrame(bitmap);
                        }
                        if (oldBitmap != bitmap)
                            oldBitmap.Dispose();
                        oldBitmap = bitmap;
                        if ((NextFrameInsertPosition & 63) == 0)
                        {
                            bitmapHandler.writer.Flush();
                            GC.Collect();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            bitmapHandler?.writer?.Close();
            bitmapHandler.writer.Dispose();
        }
        public BitmapHandler(string OutputPath, BasicCapture basicCapture)
        {
            this.OutputPath = OutputPath;
            this.basicCapture = basicCapture;
            ThreadPool.QueueUserWorkItem(new WaitCallback((s)=> 
            {
                var consumer = ConsumeData(s);
                consumer.Wait();
            }), this);
        }
    }
}