using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using RedisEndpoint;
using System;
using System.Collections.Generic;

namespace AudioProcessor
{

    public class AudioProcessor
    {
        //All values are in ms
        const int CAPTURE_LATENCY = 10, PROCESS_WINDOW_LENGTH = 10, LFE_CUTOFF = 125;

        private float[] blockBuffer;
        private int channelNum, systemSampleRate, hitCount = 0;
        private LowpassFilter lpf;
        private Localisationer localisationer;
        private SimpleImpulseDetector MonoImpulseDetector, LFEImpulseDetector;
        private List<float[]> monoBuffers = new List<float[]>();


        public AudioProcessor(Publisher publisher)
        {
            using WasapiCapture capture = new WasapiLoopbackCapture(CAPTURE_LATENCY);
            capture.Initialize();
            channelNum = capture.WaveFormat.Channels;
            systemSampleRate = capture.WaveFormat.SampleRate;

            using SoundInSource captureSource =
                new SoundInSource(capture) { FillWithZeros = false };
            using SimpleNotificationSource notificationSource =
                new SimpleNotificationSource(FluentExtensions.ToSampleSource(captureSource))
                {
                    Interval = PROCESS_WINDOW_LENGTH
                };

            InitializeMonoBuffers(monoBuffers, channelNum, notificationSource.BlockCount);
            blockBuffer = new float[notificationSource.BlockCount * channelNum];
            lpf = new LowpassFilter(systemSampleRate, LFE_CUTOFF);
            MonoImpulseDetector =
                new SimpleImpulseDetector(monoBuffers, lfeProvided: false, biQuadFilter: lpf);
            localisationer = new Localisationer(monoBuffers);
            if (channelNum > 2)
            {
                LFEImpulseDetector =
                        new SimpleImpulseDetector(monoBuffers, lfeProvided: true);
            }

            capture.DataAvailable += (s, e) =>
            {
                while (notificationSource.Read(blockBuffer, 0, notificationSource.BlockCount * channelNum) > 0)
                {
                    monoBuffers = Deinterlacing(monoBuffers,
                                                blockBuffer,
                                                channelNum);
                    if (LFEImpulseDetector != null)
                    {
                        bool m = MonoImpulseDetector.Predict();
                        bool l = LFEImpulseDetector.Predict();
                        if (m || l)
                        {
                            double angle = localisationer.GetLoudestAngle();
#if DEBUG
                            Console.Clear();
                            Console.WriteLine($"LFE Level: {LFEImpulseDetector.CurrentReading:F3}, LFE Threshold: {LFEImpulseDetector.CurrentThreshold:F3}");
                            Console.WriteLine($"Mixed Level: {MonoImpulseDetector.CurrentReading:F3}, Mixed Threshold: {MonoImpulseDetector.CurrentThreshold:F3}");
                            Console.WriteLine($"Impulse Detected - Mono:{m}, LFE:{l}, Angle: {angle:F3}, Hit Count:{hitCount}");
#endif
                            if (publisher != null)
                                publisher.Publish("IMPULSE", $"{m}|{l}|{angle:F3}");
                            hitCount++;
                        }
                    }
                    else
                    {
                        if (MonoImpulseDetector.Predict())
                        {
                            double angle = localisationer.GetLoudestAngle();
#if DEBUG
                            Console.Clear();
                            Console.WriteLine($"Level: {MonoImpulseDetector.CurrentReading:F3}, Threshold: {MonoImpulseDetector.CurrentThreshold:F3}");
                            Console.WriteLine($"Impulse Detected - Mono, Angle:{angle:F3}, Hit Count:{hitCount}");
#endif
                            if (publisher != null)
                                publisher.Publish("IMPULSE", $"True|False|{angle:F3}");
                            hitCount++;
                        }
                    }
                }
            };
            StartCapturingAndHold(capture);
        }
        void InitializeMonoBuffers(List<float[]> monoBuffers, int channelNum, int blockCount)
        {
            for (int channel = 0; channel < channelNum; channel++)
                monoBuffers.Add(new float[blockCount]);
        }

        List<float[]> Deinterlacing(List<float[]> monoBuffers, float[] blockBuffer, int channelNum)
        {
            for (int block = 0; block < blockBuffer.Length; block++)
            {
                monoBuffers[block % channelNum][block / channelNum] = blockBuffer[block];
            }
            return monoBuffers;
        }

        void StartCapturingAndHold(WasapiCapture capture)
        {
            capture.Start();
#if DEBUG
            Console.WriteLine("Start Capturing...");
            Console.WriteLine("Input Format: " + capture.WaveFormat.ToString());
#endif
            _ = Console.ReadKey();
            capture.Stop();
        }

        public static int Main()
        {
            Publisher publisher = new Publisher("localhost", 6380);
            AudioProcessor audioProcessor = new AudioProcessor(publisher);
            return 0;
        }
    }
}
