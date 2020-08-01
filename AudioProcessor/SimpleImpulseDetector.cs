using CSCore.DSP;
using System;
using System.Collections.Generic;
using System.Linq;


namespace AudioProcessor
{

    class SimpleImpulseDetector
    {
        private const int LFE_CHANNEL_NUMBER = 3;

        private bool lfeProvided;
        private double currentThreshold = -double.NaN, lastThreshold = 0,
            currentReading = 0, noiseThreshold;
        private double[] mixedMonoLevel;
        private float[] filterBuffer;

        private readonly List<float[]> monoBuffers;
        private readonly BiQuad biQuadFilter;

        public double alpha, margin;
        public double CurrentThreshold => currentThreshold;
        public double CurrentReading => currentReading;

        public SimpleImpulseDetector(List<float[]> monoBuffers, bool lfeProvided,
            BiQuad biQuadFilter = null, double alpha = 0.7, double margin = 3.5, double noiseThreshold = -40)
        {
            this.alpha = alpha;
            this.biQuadFilter = biQuadFilter;
            this.lfeProvided = lfeProvided;
            this.margin = margin;
            this.monoBuffers = monoBuffers;
            this.noiseThreshold = noiseThreshold;

            mixedMonoLevel = new double[monoBuffers[0].Length];
            filterBuffer = new float[mixedMonoLevel.Length];
        }

        private void MonoMixing()
        {
            for (int block = 0; block < filterBuffer.Length; block++)
            {
                filterBuffer[block] = 0;
                for (int channel = 0; channel < monoBuffers.Count; channel++)
                    filterBuffer[block] += monoBuffers[channel][block];
                filterBuffer[block] /= monoBuffers.Count-1;
            }
        }

        private void CastToDoubleArray()
        {
            for (int block = 0; block < filterBuffer.Length; block++)
            {
                mixedMonoLevel[block] = filterBuffer[block];
            }
        }

        private double GetCurrentReading()
        {
            for (int block = 0; block < mixedMonoLevel.Length; block++)
            {
                mixedMonoLevel[block] = Math.Pow(mixedMonoLevel[block], 2);
            }

            return 20 * Math.Log(Math.Sqrt(mixedMonoLevel.Average()));
        }

        public bool Predict()
        {
            bool hit;

            if (!lfeProvided) MonoMixing();
            else for (int block = 0; block < mixedMonoLevel.Length; block++)
                {
                    filterBuffer[block] = monoBuffers[LFE_CHANNEL_NUMBER][block];
                }

            if (biQuadFilter != null)
            {
                biQuadFilter.Process(filterBuffer);
            }

            CastToDoubleArray();
            currentReading = GetCurrentReading();

            if (currentThreshold.Equals(double.NaN))
            {
                currentThreshold = currentReading + margin;
                hit = false;
            }
            else
            {
                currentThreshold = (1d - alpha) * lastThreshold + alpha * (currentReading + margin);
                hit = currentReading > currentThreshold && currentReading >= noiseThreshold;
            }
            lastThreshold = currentThreshold;

            return hit;
        }

    }
}
