using System;
using System.Threading;

namespace ImageProcessModule
{
    public class RedImpulseDetection : ImageProcessBase
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
                return 1;
            }
        }
        protected override ImageScaleType ImageScale
        {
            get
            {
                return ImageScaleType.Half;
            }
        }
        private bool IsStopRunning = false;
        private const int RecordHistorySize = 60;
        private double[,] HistoryColorAvg = new double[RecordHistorySize, 3];
        private int HistoryIndex = 0;
        ~RedImpulseDetection()
        {
            IsStopRunning = true;
        }

        protected override void ImageHandler(object args)
        {
            while (!IsStopRunning)
            {
                while (!IsProcessingData)
                    Thread.Sleep(1);
                ulong[] Channels = new ulong[4];
                int PixelCounts = Data.Rows * Data.Cols;
                int DataCounts = 4 * PixelCounts;
                unsafe
                {
                    for (int i = 0; i < DataCounts; ++i)
                        Channels[i % 4] += ((byte*)Data.DataPointer)[i];
                }
                for (int i = 0; i < 3; ++i)
                    HistoryColorAvg[HistoryIndex, i] = Channels[i] / (double)PixelCounts;

                //Console.WriteLine("{0}, {1}, {2}", HistoryColorAvg[HistoryIndex, 0], HistoryColorAvg[HistoryIndex, 1], HistoryColorAvg[HistoryIndex, 2]);
                double Fraction_R_B = HistoryColorAvg[HistoryIndex, 2] / HistoryColorAvg[HistoryIndex, 0],
                    Fraction_R_G = HistoryColorAvg[HistoryIndex, 2] / HistoryColorAvg[HistoryIndex, 1],
                    Fraction_B_G = HistoryColorAvg[HistoryIndex, 0] / HistoryColorAvg[HistoryIndex, 1];
                if (Fraction_R_B > Fraction_B_G && Fraction_R_G > 1 / (Fraction_B_G) //Red Channel is more than Blue/Green
                    && Fraction_R_B > 2 && Fraction_R_G > 2 //If this is red impluse, it should be pure red 
                    && HistoryColorAvg[HistoryIndex, 0] < 80 && HistoryColorAvg[HistoryIndex, 1] < 80)
                {
                    double[] AvgHistory = new double[3];
                    for (int i = 0; i < RecordHistorySize; ++i)
                    {
                        for (int j = 0; j < 3; ++j)
                            AvgHistory[j] += HistoryColorAvg[i, j];
                    }
                    for (int j = 0; j < 3; ++j)
                        AvgHistory[j] /= RecordHistorySize;
                    if (AvgHistory[2] / AvgHistory[0] < Fraction_R_B && AvgHistory[2] / AvgHistory[1] < Fraction_R_G)
                        Console.WriteLine("Attacked");
                }
                HistoryIndex = ++HistoryIndex % RecordHistorySize;
                IsProcessingData = false;
            }
        }

    }
}
