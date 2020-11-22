using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using ImageProcessModule;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
namespace ScreenCapture
{
    public class LocalCapture : CaptureMethod
    {
        /*Const Variable*/
        const int PreCreateMatCount = 30;
        /*Const Variable*/

        /*Screen Capture Variable*/
        Factory1 factory;
        Adapter1 adapter;
        Device device;
        Output output;
        Output1 output1;
        int width;
        int height;
        Texture2DDescription textureDesc;
        Texture2D screenTexture;
        OutputDuplication duplicatedOutput;
        /*Screen Capture Variable*/

        /*Multi-Thread Variable*/
        CancellationTokenSource ThreadStopSignal;
        BitmapBuffer bitmapBuffer;
        /*Multi-Thread Variable*/

        /// <summary>
        /// Create LocalCapture to capture screen (by DXGI.)
        /// </summary>
        /// <param name="bitmapBuffer">Communication pipe with other threads. It stores some processing Mat and some unused Mat</param>
        /// <param name="numOutput"># of output device (i.e. monitor).</param>
        /// <param name="numAdapter"># of output adapter (i.e. iGPU). </param>
        public LocalCapture(BitmapBuffer bitmapBuffer, int numOutput = 0, int numAdapter = 0)
        {
            // Create DXGI Factory1
            factory = new Factory1();
            adapter = factory.GetAdapter1(numAdapter);

            // Create device from Adapter
            device = new Device(adapter);

            // Get DXGI.Output
            output = adapter.GetOutput(numOutput);
            output1 = output.QueryInterface<Output1>();


            // Width/Height of desktop to capture
            width = ((Rectangle)output.Description.DesktopBounds).Width;
            height = ((Rectangle)output.Description.DesktopBounds).Height;

            // Create Staging texture CPU-accessible
            textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            screenTexture = new Texture2D(device, textureDesc);

            // Duplicate the output
            duplicatedOutput = output1.DuplicateOutput(device);

            //Create enough UnusedMat
            for (int i = 0; i < PreCreateMatCount; ++i)
                bitmapBuffer.PushUnusedMat(CreateSuitableMat());

            //Save the buffer
            this.bitmapBuffer = bitmapBuffer;
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
            ThreadStopSignal?.Cancel();
            ThreadStopSignal = new CancellationTokenSource();
            new Thread(WorkerThread).Start();
        }
        /// <summary>
        /// Call Stop to deactivate image capture.
        /// </summary>
        public void Stop()
        {
            ThreadStopSignal?.Cancel();
        }

        private void WorkerThread()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            CancellationTokenSource cancellation = ThreadStopSignal;
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    SharpDX.DXGI.Resource screenResource;
                    OutputDuplicateFrameInformation duplicateFrameInformation;

                    // Try to get duplicated frame within given time
                    duplicatedOutput.AcquireNextFrame(10000, out duplicateFrameInformation, out screenResource);

                    // copy resource into memory that can be accessed by the CPU
                    using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                        device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                    // Get the desktop capture texture
                    var mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, MapFlags.None);

                    // Create Mat
                    var NewMat = bitmapBuffer.GetUnusedMat();
                    if (NewMat == null)
                        NewMat = CreateSuitableMat();

                    // Copy Image to Mat
                    Copy_32Argb_ImageTo_32Argb_Mat(in mapSource, in NewMat);

                    // Send to buffer
                    bitmapBuffer.PushProcessingMat(NewMat);

                    // Release source lock
                    device.ImmediateContext.UnmapSubresource(screenTexture, 0);
                    screenResource.Dispose();
                    duplicatedOutput.ReleaseFrame();
                }
                catch (SharpDXException em)
                {
                    if (em.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                    {
                        throw em;
                    }
                }
            }
        }
        /// <summary>
        /// Copy Image from Texture2D to Mat.
        /// Be aware that the DestinationMat should be initialized. The width and height of Mat should fit the size in Bitmap.
        /// </summary>
        /// <param name="SrcImagePtr">Source. Data will not be changed in this function.</param>
        /// <param name="DestinationMat">Destination. Data should be fully initialized. DestinationMat will be changed in this funtion.</param>
        private void Copy_32Argb_ImageTo_32Argb_Mat(in DataBox SrcImagePtr, in Mat DestinationMat)
        {
            // Copy pixels from screen capture Texture to GDI bitmap
            var destPtr = DestinationMat.DataPointer;
            var sourcePtr = SrcImagePtr.DataPointer;
            for (int y = 0; y < height; y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, width * 4);

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, SrcImagePtr.RowPitch);
                destPtr = IntPtr.Add(destPtr, width * 4);
            }
        }
    }
}