using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;



namespace AudioProcessor
{
    class Localisationer
    {
        private readonly float[] channelSettings = new float[] { 30, -30, 0, float.NaN, 110, -110 };

        private readonly List<float[]> monoBuffers;

        public Localisationer(List<float[]> monoBuffers)
        {
            this.monoBuffers = monoBuffers;
        }

        private static Vector2 ToCartesian(float angle)
        {
            return new Vector2((float)Math.Cos(angle * Math.PI / 180d),
                (float)Math.Sin(angle * Math.PI / 180d));
        }
        private List<Vector2> VectorizeSignals(List<float[]> monoBuffers)
        {
            List<Vector2> TempVectors = new List<Vector2>();
            for (int channel = 0; channel < monoBuffers.Count; channel++)
            {
                if (!float.IsNaN(channelSettings[channel]))
                {
                    float avgLevel = monoBuffers[channel].Average();
                    TempVectors.Add(Vector2.Multiply((float)Math.Pow(avgLevel, 2),
                        ToCartesian(channelSettings[channel])));
                }
            }

            return TempVectors;
        }
        private Vector2 MixSignal(List<Vector2> vectorized)
        {
            Vector2 tempVector = new Vector2(0, 0);
            for (int channel = 0; channel < vectorized.Count; channel++) tempVector += vectorized[channel];
            return tempVector;
        }
        public double GetLoudestAngle()
        {
            Vector2 mixedSignal = MixSignal(VectorizeSignals(monoBuffers));
            double angle = Math.Acos(Vector2.Dot(new Vector2(1, 0), mixedSignal) / mixedSignal.Length()) * 180 / Math.PI;
            return mixedSignal.Y > 0 ? angle : 360 - angle;
        }
    }
}
