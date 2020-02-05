//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using Composition.WindowsRuntimeHelpers;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.IO;
using SharpDX.WIC;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.UI.Composition;

namespace CaptureSampleCore
{
    public class BasicCapture : IDisposable
    {
        public double RecordFrameRate = -1;
        private long LastTick;
        public Process proc;
        public ulong StartRecordTime;
        private GraphicsCaptureItem item;
        private Direct3D11CaptureFramePool framePool;
        private GraphicsCaptureSession session;
        private SizeInt32 lastSize;

        private IDirect3DDevice device;
        private SharpDX.Direct3D11.Device d3dDevice;
        private SharpDX.DXGI.SwapChain1 swapChain;

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hwnd, out Rect lpRect);
        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
        private Rect BorderSize = new Rect();
        private bool BorderSizeInited = false;

        public Action<Bitmap, ulong> OnBitmapCreate;
        private void OnWindowsSizeChange()
        {
            if (proc == null)
                return;
            BorderSizeInited = true;
            Rect WindowsPositionInScreen, WindowsPositionInRef;
            GetWindowRect(proc.MainWindowHandle, out WindowsPositionInScreen);
            GetClientRect(proc.MainWindowHandle, out WindowsPositionInRef);
            int TotalBorderWidthSize =
                (WindowsPositionInScreen.Right - WindowsPositionInScreen.Left)
                - (WindowsPositionInRef.Right - WindowsPositionInRef.Left);
            int TotalBorderHeightSize =
                            (WindowsPositionInScreen.Bottom - WindowsPositionInScreen.Top)
                            - (WindowsPositionInRef.Bottom - WindowsPositionInRef.Top);
            int _BorderSize = TotalBorderWidthSize / 2;
            int TitleBarSize = TotalBorderHeightSize - _BorderSize;
            BorderSize.Left = BorderSize.Right = BorderSize.Bottom = _BorderSize;
            BorderSize.Top = TitleBarSize;

            int Width = WindowsPositionInRef.Right - WindowsPositionInRef.Left - BorderSize.Left - BorderSize.Right,
                Height = WindowsPositionInRef.Bottom - WindowsPositionInRef.Top - BorderSize.Top - BorderSize.Bottom;
            if (Width % 2 != 0)
                BorderSize.Right--;
            if (Height % 2 != 0)
                BorderSize.Top++;
        }
        public BasicCapture(IDirect3DDevice d, GraphicsCaptureItem i)
        {
            item = i;
            device = d;
            d3dDevice = Direct3D11Helper.CreateSharpDXDevice(device);

            var dxgiFactory = new SharpDX.DXGI.Factory2();
            var description = new SharpDX.DXGI.SwapChainDescription1()
            {
                Width = item.Size.Width,
                Height = item.Size.Height,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SharpDX.DXGI.SampleDescription()
                {
                    Count = 1,
                    Quality = 0
                },
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = SharpDX.DXGI.Scaling.Stretch,
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
                AlphaMode = SharpDX.DXGI.AlphaMode.Premultiplied,
                Flags = SharpDX.DXGI.SwapChainFlags.None
            };
            swapChain = new SharpDX.DXGI.SwapChain1(dxgiFactory, d3dDevice, ref description);

            framePool = Direct3D11CaptureFramePool.Create(
                device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                i.Size);
            session = framePool.CreateCaptureSession(i);
            lastSize = i.Size;

            framePool.FrameArrived += OnFrameArrived;
        }

        public void Dispose()
        {
            session?.Dispose();
            framePool?.Dispose();
            swapChain?.Dispose();
            d3dDevice?.Dispose();
        }

        public void StartCapture()
        {
            session.StartCapture();
        }

        public ICompositionSurface CreateSurface(Compositor compositor)
        {
            return compositor.CreateCompositionSurfaceForSwapChain(swapChain);
        }

        private void GetBitmap(Texture2D texture)
        {
            ulong timestamp = GetTickCount64() - StartRecordTime;
            // Create texture copy
            var copy = new Texture2D(d3dDevice, new Texture2DDescription
            {
                Width = texture.Description.Width,
                Height = texture.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = texture.Description.Format,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            });

            // Copy data
            d3dDevice.ImmediateContext.CopyResource(texture, copy);

            // Map image from GPU
            var dataBox = d3dDevice.ImmediateContext.MapSubresource(copy, 0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out DataStream stream);
            
            //Get Pixel Address & Row Size
            var rect = new DataRectangle
            {
                DataPointer = stream.DataPointer,
                Pitch = dataBox.RowPitch
            };

            var factory = new ImagingFactory();
            var format = PixelFormat.Format32bppPBGRA;

            int NewImageWidth = copy.Description.Width - BorderSize.Left - BorderSize.Right,
                NewImageHeight = copy.Description.Height - BorderSize.Top - BorderSize.Bottom;
            if (NewImageWidth < 0 || NewImageHeight < 0)
                return;
            //Create New Image
            Bitmap bmp = new Bitmap(factory, NewImageWidth, NewImageHeight, format, BitmapCreateCacheOption.CacheOnLoad);
            //Get Address of New Image with Writing Lock
            BitmapLock bLock = bmp.Lock(BitmapLockFlags.Write);
            
            //Fetch Address from bLock
            IntPtr DestPtr = bLock.Data.DataPointer;
            //Fetch Starting Image from old image
            int NumsBytesOfSkipPixel =
                rect.Pitch * BorderSize.Top //Skip TitleBar (Pitch: Bytes per Row. 64b align)
                + BorderSize.Left * 4; //Skip Left Borderline with 4 Bytes per Pixel
            IntPtr SourcePtr = IntPtr.Add(dataBox.DataPointer, NumsBytesOfSkipPixel);

            for (int i = 0; i < NewImageHeight; ++i )
            {
                Utilities.CopyMemory(DestPtr, SourcePtr, NewImageWidth * 4);
                SourcePtr = IntPtr.Add(SourcePtr, rect.Pitch);
                DestPtr = IntPtr.Add(DestPtr, bLock.Data.Pitch);
            }

            //Release Write Lock
            bLock.Dispose();

            //Ummapped Image
            d3dDevice.ImmediateContext.UnmapSubresource(copy, 0);
            copy.Dispose();

            OnBitmapCreate?.Invoke(bmp, timestamp);
        }


        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            var newSize = false;
            using (var frame = sender.TryGetNextFrame())
            {
                double DeltaTick = frame.SystemRelativeTime.Ticks - LastTick;
                LastTick = frame.SystemRelativeTime.Ticks;
                double TempRecordFrameRate = 1 / (DeltaTick / TimeSpan.TicksPerSecond);
                if (RecordFrameRate > 0)
                    RecordFrameRate = 0.9 * RecordFrameRate + TempRecordFrameRate * 0.1; //Low Pass Filter
                else
                    RecordFrameRate = TempRecordFrameRate;
                if (frame.ContentSize.Width != lastSize.Width ||
                    frame.ContentSize.Height != lastSize.Height)
                {
                    OnWindowsSizeChange();
                    // The thing we have been capturing has changed size.
                    // We need to resize the swap chain first, then blit the pixels.
                    // After we do that, retire the frame and then recreate the frame pool.
                    newSize = true;
                    lastSize = frame.ContentSize;
                    swapChain.ResizeBuffers(
                        2, 
                        lastSize.Width, 
                        lastSize.Height, 
                        SharpDX.DXGI.Format.B8G8R8A8_UNorm, 
                        SharpDX.DXGI.SwapChainFlags.None);
                }

                if (!BorderSizeInited)
                    OnWindowsSizeChange();
                using (var backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
                using (var bitmap = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface))
                {
                    d3dDevice.ImmediateContext.CopyResource(bitmap, backBuffer);
                    if (!newSize && StartRecordTime != 0)
                        GetBitmap(bitmap);
                }
            } // Retire the frame.

            swapChain.Present(0, SharpDX.DXGI.PresentFlags.None);

            if (newSize)
            {
                framePool.Recreate(
                    device,
                    DirectXPixelFormat.B8G8R8A8UIntNormalized,
                    2,
                    lastSize);
            }
        }
    }
}
