using Composition.WindowsRuntimeHelpers;
using Emgu.CV;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Windows.Graphics.Capture;

namespace WPFCaptureSample.ScreenCapture
{
    class OnFrameArrivedEvent
    {
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int Height
            {
                get
                {
                    return Top + Bottom;
                }
            }
            public int Width
            {
                get
                {
                    return Left + Right;
                }
            }
        }
        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hwnd, out Rect lpRect);

        private static Rect BorderLineSize;
        public static long LastWidthSize = -1, LastHeightSize = -1;

        public static IntPtr TargetHwnd;

        private static void UpdateBorderSize()
        {
            Rect WindowsPositionInScreen, WindowsPositionInRef;
            GetWindowRect(TargetHwnd, out WindowsPositionInScreen);
            GetClientRect(TargetHwnd, out WindowsPositionInRef);
            int WindowWidthSize = (WindowsPositionInScreen.Right - WindowsPositionInScreen.Left),
                ContentWidthSize = (WindowsPositionInRef.Right - WindowsPositionInRef.Left),
                BorderWidthSize = WindowWidthSize - ContentWidthSize;
            int WindowHeightSize = (WindowsPositionInScreen.Bottom - WindowsPositionInScreen.Top),
                ContentHeightSize = (WindowsPositionInRef.Bottom - WindowsPositionInRef.Top),
                BorderHeightSize = WindowHeightSize - ContentHeightSize;
            int BorderSize = BorderWidthSize / 2;
            int TitleBarSize = Math.Max(BorderHeightSize - BorderSize, 0);
            BorderLineSize.Left = BorderLineSize.Right = BorderLineSize.Bottom = BorderSize;
            BorderLineSize.Top = TitleBarSize;

            if (WindowWidthSize % 2 != 0)
                BorderLineSize.Right--;
            if ((WindowHeightSize - BorderHeightSize) % 2 != 0)
                BorderLineSize.Top++;
        }

        public static void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            using (var frame = sender.TryGetNextFrame())
            {
                bool IsFrameNewSize = !frame.ContentSize.Width.Equals(LastWidthSize)
                    || !frame.ContentSize.Height.Equals(LastHeightSize);
                if (IsFrameNewSize)
                    UpdateBorderSize();
                using (var texture2D = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface))
                {
                    var d3dDevice = texture2D.Device;
                    var copy = new Texture2D(d3dDevice, new Texture2DDescription
                    {
                        Width = texture2D.Description.Width,
                        Height = texture2D.Description.Height,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = texture2D.Description.Format,
                        Usage = ResourceUsage.Staging,
                        SampleDescription = new SampleDescription(1, 0),
                        BindFlags = BindFlags.None,
                        CpuAccessFlags = CpuAccessFlags.Read,
                        OptionFlags = ResourceOptionFlags.None
                    });

                    // Copy data
                    d3dDevice.ImmediateContext.CopyResource(texture2D, copy);

                    // Map image from GPU
                    var dataBox = d3dDevice.ImmediateContext.MapSubresource(copy, 0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out DataStream stream);
                    
                    var rect = new DataRectangle
                    {
                        DataPointer = stream.DataPointer,
                        Pitch = dataBox.RowPitch
                    };

                    int clipped_width = frame.ContentSize.Width - BorderLineSize.Width,
                        clipped_height = frame.ContentSize.Height - BorderLineSize.Height;

                    if (FrameDispatcher.UnusedMatBuffer == null)
                    {
                        FrameDispatcher.UnusedMatBuffer = new BufferBlock<Mat>();
                        for (int i = 0; i < FrameDispatcher.InitMatBufferSize; ++i)
                            FrameDispatcher.UnusedMatBuffer.Post(new Mat(clipped_height, clipped_width, Emgu.CV.CvEnum.DepthType.Cv8U, 4));
                    }
                    Mat mat;
                    if (FrameDispatcher.UnusedMatBuffer.Count == 0)
                        mat = new Mat(clipped_height, clipped_width, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
                    else
                    {
                        mat = FrameDispatcher.UnusedMatBuffer.Receive();
                        if (mat.Rows != clipped_height || mat.Cols != clipped_width)
                        {
                            mat.Dispose();
                            mat = new Mat(clipped_height, clipped_width, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
                        }
                    }

                    for (int i = BorderLineSize.Top; i < frame.ContentSize.Height - BorderLineSize.Bottom; ++i)
                    {
                        IntPtr RowStartAddress = IntPtr.Add(rect.DataPointer, rect.Pitch * i);  //Skip TitleBar (Pitch: Bytes per Row. 64b align)
                        IntPtr PixelStartAddress = IntPtr.Add(RowStartAddress, 4 * BorderLineSize.Left);    //Skip Left Borderline with 4 Bytes per Pixel
                        int BytesPerRow = clipped_width * 4;
                        Utilities.CopyMemory(mat.DataPointer + ((i - BorderLineSize.Top) * mat.Cols) * mat.ElementSize, PixelStartAddress, BytesPerRow);
                    }
                    d3dDevice.ImmediateContext.UnmapSubresource(copy, 0);
                    copy.Dispose();
                    FrameDispatcher.WaitingMatBuffer.Post(mat);
                }
            }
        }
    }
}
