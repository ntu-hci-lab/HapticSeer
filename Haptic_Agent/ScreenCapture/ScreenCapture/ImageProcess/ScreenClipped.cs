using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFCaptureSample.ScreenCapture.ImageProcess
{
    class ScreenClipped : ImageProcessBase
    {
        private bool IsStopRunning = false;
        protected override double Clipped_Left
        {
            get
            {
                return 0.913;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return 0.024f;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return 0.976;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
                return 0.0571f;
            }
        }
        protected override double Scale_Width
        {
            get
            {
                return 1;
            }
        }
        protected override double Scale_Height
        {
            get
            {
                return 1;
            }
        }
        private Mat BackgroundRemovalImage = new Mat();

        private static void ElimateBackgroundWithBoundTest(in Mat InputImage, ref Mat OutputImage, in Color[] ItemColor, in Color[] BorderColor, in int BorderSize)
        {
            int[,] Label = new int[InputImage.Width, InputImage.Height];
            if (!InputImage.Size.Equals(OutputImage.Size))
            {
                OutputImage.Dispose();
                OutputImage = new Mat(OutputImage.Size, DepthType.Cv8U, 1);
            }
            ItemColor[0].ToArgb
            unsafe
            {
                byte* InputImageColor = (byte*)InputImage.DataPointer;
            }
        }
        protected override void ImageHandler(object args)
        {
            MCvScalar scalar = new MCvScalar(0);
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                if (!BackgroundRemovalImage.Size.Equals(Data.Size))
                {
                    BackgroundRemovalImage.Dispose();
                    BackgroundRemovalImage = new Mat(Data.Size, DepthType.Cv8U, 1);
                }
                BackgroundRemovalImage.SetTo(scalar);

                unsafe
                {
                    byte* InputImageData = (byte*)Data.DataPointer;
                    byte* OutputImageData = (byte*)BackgroundRemovalImage.DataPointer;
                    for (int i = 0, height = Data.Size.Height, offset = 0; i < height; ++i)
                    {
                        for (int j = 0, width = Data.Size.Width; j < width; ++j)
                        {
                            if (InputImageData[offset + 0] > 200 &&
                                InputImageData[offset + 1] > 200 &&
                                InputImageData[offset + 2] > 200)
                                OutputImageData[offset >> 2] = 255;
                            offset += 4;
                        }
                    }
                }
                //CvInvoke.Imwrite("O:\\Out.png", BackgroundRemovalImage);
                IsProcessingData = false;
            }
        }
    }
}
