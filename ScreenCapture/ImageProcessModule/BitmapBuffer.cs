using Emgu.CV;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
namespace ImageProcessModule
{
    public class BitmapBuffer
    {
        /// <summary>
        /// The buffer stores the Mat that is available
        /// </summary>
        private BufferBlock<Mat> UnusedBuffer = new BufferBlock<Mat>();
        /// <summary>
        /// The buffer stores the Mat that is waiting for processing
        /// </summary>
        private BufferBlock<Mat> ProcessingBuffer = new BufferBlock<Mat>();

        /*Multi-Thread Variable*/
        CancellationTokenSource ThreadStopSignal;
        /*Multi-Thread Variable*/

        /// <summary>
        /// Push Mat to UnusedBuffer.
        /// Call this function if the Mat is available now.
        /// </summary>
        /// <param name="UnusedMat">Available Mat</param>
        public void PushUnusedMat(Mat UnusedMat)
        {
            if (UnusedMat != null && !UnusedMat.Size.IsEmpty)
                UnusedBuffer.Post(UnusedMat);
        }
        /// <summary>
        /// Return a unused Mat.
        /// Call this function to acquire a unused Mat.
        /// If there is no available Mat, it will return null.
        /// </summary>
        /// <returns>Available Mat. Return null when there is no available Mat.</returns>
        public Mat GetUnusedMat()
        {
            if (UnusedBuffer.Count == 0)
                return null;
            return UnusedBuffer.Receive();
        }

        /// <summary>
        /// Push Mat to ProcessingBuffer.
        /// Call this function if a Mat is waiting for processing.
        /// </summary>
        /// <param name="ProcessingMat">Mat that contains data</param>
        public void PushProcessingMat(Mat ProcessingMat)
        {
            ProcessingBuffer.Post(ProcessingMat);
        }
        /// <summary>
        /// Get Mat which contains new frame
        /// </summary>
        public async Task<Mat> GetProcessingBitmap()
        {
            await ProcessingBuffer.OutputAvailableAsync().ConfigureAwait(false);
            return ProcessingBuffer.Receive();
        }
        /// <summary>
        /// Force disable the buffer system
        /// </summary>
        public void StopBuffer()
        {
            ProcessingBuffer.Complete();
            UnusedBuffer.Complete();
        }
        /// <summary>
        /// Start automatic dispatch new frame to class inherited from Class<ImageProcessBase> 
        /// </summary>
        public void StartDispatchToImageProcessBase()
        {
            ThreadStopSignal = new CancellationTokenSource();
            new Thread(Dispatcher).Start();
        }
        /// <summary>
        /// Stop automatic dispatch new frame to objects whose class are inherited from Class<ImageProcessBase> 
        /// </summary>
        public void StopDispatchToImageProcessBase()
        {
            ThreadStopSignal?.Cancel();
        }

        private void Dispatcher()
        {
            // Check the Thread stop signal & Check is Buffer still working 
            while (!ThreadStopSignal.IsCancellationRequested && !ProcessingBuffer.Completion.IsCompleted)
            {
                // Acquire new frame
                Task<Mat> NewProcessingImgTask = GetProcessingBitmap();
                while (!NewProcessingImgTask.IsCompleted)
                    Thread.Sleep(1);
                Mat NewMat = NewProcessingImgTask.Result;

                // Dispatch the frame to ImageProcessBase
                ImageProcessBase.TryUpdateAllData(in NewMat);

                // After the dispatch, the frame is available again.
                // Send it back to Unused Buffer
                PushUnusedMat(NewMat);
            }
        }
    }
}