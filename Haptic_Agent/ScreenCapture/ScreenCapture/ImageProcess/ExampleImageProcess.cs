using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFCaptureSample.ScreenCapture.ImageProcess
{
    class ExampleImageProcess : ImageProcessBase
    {
        protected override double Clipped_Left
        {
            get
            {
                return 0;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return 0;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return 1;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
                return 1f;
            }
        }
        protected override double Scale_Width
        {
            get
            {
                return 1f;
            }
        }
        protected override double Scale_Height
        {
            get
            {
                return 1f;
            }
        }

        private volatile bool IsStopRunning = false;
        ~ExampleImageProcess()
        {
            IsStopRunning = true;
        }
        protected override void ImageHandler(object args)
        {
            Mat Output = new Mat();
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                if (!Output.Size.Equals(Data.Size))
                {
                    Output.Dispose();
                    Output = new Mat(Data.Rows, Data.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                }
                //Image Data -> Data
                //Data;
                unsafe
                {
                    byte* ptr = (byte*)Data.DataPointer;//ARGB
                    byte* outPtr = (byte*)Output.DataPointer;
                    Mat[] BGRA = Data.Split();
                    outPtr[0] = ptr[0];
                    outPtr[1] = ptr[4];
                    //0> B 1->G 2->R 3->A 4->B 
                }
                IsProcessingData = false;
            }
        }
    }
}
