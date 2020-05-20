using System;
using System.Drawing.Imaging;
using System.Threading;
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
        const int BitmapCount = 30;
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


        /// <param name="numOutput"># of output device (i.e. monitor).</param>
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
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            CancellationTokenSource cancellation = ThreadStopSignal;
            var boundsRect = new System.Drawing.Rectangle(0, 0, width, height);
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

                    // Create Drawing.Bitmap
                    var bitmap = bitmapBuffer.GetUnusedBitmap();
                    if (bitmap == null)
                        bitmap = CreateSuitableBitmap();

                    // Copy pixels from screen capture Texture to GDI bitmap
                    var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    var sourcePtr = mapSource.DataPointer;
                    var destPtr = mapDest.Scan0;
                    for (int y = 0; y < height; y++)
                    {
                        // Copy a single line 
                        Utilities.CopyMemory(destPtr, sourcePtr, width * 4);

                        // Advance pointers
                        sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                        destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                    }

                    // Release Bitmap lock
                    bitmap.UnlockBits(mapDest);
                    // Send to buffer
                    bitmapBuffer.PushProcessingBitmap(bitmap);

                    //Release source lock
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
    }
}