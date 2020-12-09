using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using RedisEndpoint;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace AudioProcessor
{

    public class AudioProcessor
    {
        //All values are in ms
        const int CAPTURE_LATENCY = 10, PROCESS_WINDOW_LENGTH = 10;

        private float[] blockBuffer;
        private int channelNum, systemSampleRate;
        private List<float[]> pcmBuffers = new List<float[]>();

        public readonly static string SolutionRoot = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));

        public AudioProcessor(Publisher publisher, string outlet = null)
        {
            using WasapiCapture capture = new WasapiLoopbackCapture(CAPTURE_LATENCY);
            capture.Initialize();
            channelNum = capture.WaveFormat.Channels;
            systemSampleRate = capture.WaveFormat.SampleRate;

            using SoundInSource captureSource =
                new SoundInSource(capture){ FillWithZeros = false };
            using SimpleNotificationSource notificationSource =
                new SimpleNotificationSource(FluentExtensions.ToSampleSource(captureSource))
                {
                    Interval = PROCESS_WINDOW_LENGTH
                };

            InitializeMonoBuffers(pcmBuffers, channelNum, notificationSource.BlockCount);
            blockBuffer = new float[notificationSource.BlockCount * channelNum];

            capture.DataAvailable += (s, e) =>
            {
                while (notificationSource.Read(blockBuffer, 0, notificationSource.BlockCount * channelNum) > 0)
                {
                    // Extracted audio signal
                    pcmBuffers = Deinterlacing(pcmBuffers,
                                                blockBuffer,
                                                channelNum);

                    // TODO: Implement your model
                    publisher.Publish(outlet, "OUTPUT MESSAGE TO NEXT OUTLET");
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
            Parallel.For(0, blockBuffer.Length, (block) =>
            {
                monoBuffers[block % channelNum][block / channelNum] = blockBuffer[block];
            });
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

        public static int Main(string[] args)
        {
            Publisher publisher = new Publisher("localhost", 6380);
            AudioProcessor audioProcessor = new AudioProcessor(publisher, args[0]);
            return 0;
        }
    }
}
