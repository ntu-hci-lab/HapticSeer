using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFCaptureSample.ScreenCapture.ImageProcess
{
    class BloodBarDetection : ImageProcessBase
    {
        private bool IsStopRunning = false;
        protected override double Clipped_Left
        {
            get
            {
                return 0.023;
            }
        }
        protected override double Clipped_Top
        {
            get
            {
                return 0.973;
            }
        }
        protected override double Clipped_Right
        {
            get
            {
                return 0.096;
            }
        }
        protected override double Clipped_Bottom
        {
            get
            {
                return 0.981;
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
        private int[] Label;
        private int[] LabelCounter;
        protected override void ImageHandler(object args)
        {
            MCvScalar scalar = new MCvScalar(0);
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                if (Label == null)
                {
                    Label = new int[Data.Size.Width];
                    LabelCounter = new int[Data.Size.Width];
                }
                if (Label.Length != Data.Size.Width)
                {
                    Label = new int[Data.Size.Width];
                    LabelCounter = new int[Data.Size.Width];
                }
                Array.Clear(LabelCounter, 0, LabelCounter.Length);
                unsafe
                {
                    byte* InputImageData = (byte*)Data.DataPointer;
                    //Get the mid-height offset
                    int HalfHeight = Data.Size.Height / 2;
                    int HalfHeightOffset = HalfHeight * Data.Size.Width * Data.NumberOfChannels;
                    //int Center_Offset = HalfHeightOffset + (Data.Size.Width / 2) * Data.NumberOfChannels;
                    //byte[] Center_Color = { InputImageData[Center_Offset], InputImageData[Center_Offset + 1], InputImageData[Center_Offset + 2] };
                    Label[0] = 0;
                    for (int x = 1, width = Data.Size.Width; x < width; ++x)
                    {
                        int LastOffset = HalfHeightOffset + 4 * x - 4;
                        int ThisOffset = HalfHeightOffset + 4 * x;
                        int B_Diff = InputImageData[LastOffset + 0] - InputImageData[ThisOffset + 0],
                            G_Diff = InputImageData[LastOffset + 1] - InputImageData[ThisOffset + 1],
                            R_Diff = InputImageData[LastOffset + 2] - InputImageData[ThisOffset + 2];
                        int TotalDiff = Math.Abs(B_Diff) + Math.Abs(G_Diff) + Math.Abs(R_Diff);
                        if (TotalDiff < 3)
                            Label[x] = Label[x - 1];
                        else
                            Label[x] = Label[x - 1] + 1;
                        LabelCounter[Label[x]]++;
                    }
                    int CenterLabel = Label[Data.Size.Width / 2];
                    int Center_Left_Count = 0, Center_Right_Count = 0;
                    for (int x = 0, width = Data.Size.Width; x < width; ++x)
                    {
                        if (Label[x] != CenterLabel)
                            continue;
                        else
                        {
                            if (x < Data.Size.Width / 2)
                                Center_Left_Count++;
                            else if (x > Data.Size.Width / 2)
                                Center_Right_Count++;
                        }
                    }
                    int HP, DamagedHP;
                    if (Center_Right_Count > Center_Left_Count) // Center Is Damaged
                    {
                        DamagedHP = LabelCounter[CenterLabel];
                        if (CenterLabel - 1 > 0)
                            HP = LabelCounter[CenterLabel - 1];
                        else
                            HP = 1;
                    }
                    else
                    {
                        HP = LabelCounter[CenterLabel];
                        DamagedHP = LabelCounter[CenterLabel + 1];
                    }
                    float NowHP = HP / (float)(DamagedHP + HP);
                    Console.WriteLine(NowHP);

                    }
                CvInvoke.Imwrite("O:\\Out.png", Data);
                IsProcessingData = false;
            }
        }
    }
}
