using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using WPFCaptureSample.ScreenCapture.ImageProcess;

namespace WPFCaptureSample.ScreenCapture
{
    class FrameDispatcher
    {
        public static BufferBlock<Mat> UnusedMatBuffer = null;
        public const int InitMatBufferSize = 16;
        public static BufferBlock<Mat> WaitingMatBuffer = new BufferBlock<Mat>();
        public static void StartDispatcher()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(Dispatcher));
        }
        public static void Dispatcher(object obj)
        {
            while (!WaitingMatBuffer.Completion.IsCompleted)
            {
                while (WaitingMatBuffer.Count == 0)
                    Thread.Sleep(1);
                Mat mat = WaitingMatBuffer.Receive();
                ImageProcessBase.TryUpdateAllData(mat);
                UnusedMatBuffer.Post(mat);
            }
        }
    }
}
