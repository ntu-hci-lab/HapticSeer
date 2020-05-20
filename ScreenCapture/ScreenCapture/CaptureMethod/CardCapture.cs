using System;
using System.Drawing.Imaging;
using System.Threading;

namespace ScreenCapture
{
    public class CardCapture : CaptureMethod
    {
        /*Const Variable*/
        const int BitmapCount = 30;
        /*Const Variable*/

        /*Screen Capture Variable*/

        /*Screen Capture Variable*/

        /*Multi-Thread Variable*/
        CancellationTokenSource ThreadStopSignal;
        BitmapBuffer bitmapBuffer;
        /*Multi-Thread Variable*/


        /// <param name="numOutput"># of output device (i.e. monitor).</param>
        public CardCapture(BitmapBuffer bitmapBuffer, int numOutput = 0, int numAdapter = 0)
        {
            //Create enough UnusedBitmap
            for (int i = 0; i < BitmapCount; ++i)
                bitmapBuffer.PushUnusedBitmap(CreateSuitableBitmap());
            this.bitmapBuffer = bitmapBuffer;
        }
        private System.Drawing.Bitmap CreateSuitableBitmap()
        {

            return new System.Drawing.Bitmap(width, height, PixelFormat.Format32bppArgb);
        }
        public void Start()
        {
            ThreadStopSignal?.Cancel();
            ThreadStopSignal = new CancellationTokenSource();
            new Thread(WorkerThread).Start();
        }
        public void Stop()
        {
            ThreadStopSignal?.Cancel();
        }

        private void WorkerThread()
        {
        }
    }
}