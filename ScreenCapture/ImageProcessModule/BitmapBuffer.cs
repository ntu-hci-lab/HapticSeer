using Emgu.CV;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
namespace ImageProcessModule
{
    public class BitmapBuffer
    {
        private BufferBlock<Mat> UnusedBuffer = new BufferBlock<Mat>();
        private BufferBlock<Mat> ProcessingBuffer = new BufferBlock<Mat>();

        /*Multi-Thread Variable*/
        CancellationTokenSource ThreadStopSignal;
        /*Multi-Thread Variable*/
        public void PushUnusedMat(Mat UnusedMat)
        {
            if (UnusedMat != null && !UnusedMat.Size.IsEmpty)
                UnusedBuffer.Post(UnusedMat);
        }

        public Mat GetUnusedMat()
        {
            if (UnusedBuffer.Count == 0)
                return null;
            return UnusedBuffer.Receive();
        }

        public void PushProcessingMat(Mat ProcessingMat)
        {
            ProcessingBuffer.Post(ProcessingMat);
        }

        public async Task<Mat> GetProcessingBitmap()
        {
            await ProcessingBuffer.OutputAvailableAsync().ConfigureAwait(false);
            return ProcessingBuffer.Receive();
        }

        public void StopBuffer()
        {
            ProcessingBuffer.Complete();
            UnusedBuffer.Complete();
        }

        public void StartDispatchToImageProcessBase()
        {
            ThreadStopSignal = new CancellationTokenSource();
            new Thread(Dispatcher).Start();
        }
        public void StopDispatchToImageProcessBase()
        {
            ThreadStopSignal.Cancel();
        }

        private void Dispatcher()
        {
            while (!ThreadStopSignal.IsCancellationRequested && !ProcessingBuffer.Completion.IsCompleted)
            {
                Task<Mat> NewProcessingImgTask = GetProcessingBitmap();
                while (!NewProcessingImgTask.IsCompleted)
                    Thread.Sleep(1);
                Mat NewMat = NewProcessingImgTask.Result;
                ImageProcessBase.TryUpdateAllData(in NewMat);
                PushUnusedMat(NewMat);
            }
        }
    }
}