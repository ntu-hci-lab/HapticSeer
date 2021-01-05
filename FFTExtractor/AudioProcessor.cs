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
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Pitch;


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

        int counter = 0;

        public AudioProcessor(Publisher publisher, string outlet = null)
        {
            WasapiCapture capture = new WasapiLoopbackCapture(CAPTURE_LATENCY);
            capture.Initialize();
            channelNum = capture.WaveFormat.Channels;
            systemSampleRate = capture.WaveFormat.SampleRate;

            SoundInSource captureSource =
                new SoundInSource(capture){ FillWithZeros = false };
            SimpleNotificationSource notificationSource =
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

                    CSCore.Utils.Complex[] data = new CSCore.Utils.Complex[pcmBuffers[0].Length];


                    for (int i = 0; i < pcmBuffers.Count; i++)
                    { 
                        data[i].Real = pcmBuffers[0][i];
                        data[i].Imaginary = 0;
                    }

                    //DetectPitch(pcmBuffers);

                    counter++;
                    if(counter%30 == 0)
                    {
                        Console.Clear();
                        FastFourierTransformation.Fft(data, 8);

                        //Console.WriteLine(data.Length);
                        foreach (var d in data)
                        {

                            Console.Write(Math.Round(d.Value,4) + " ");
                        }
                        Console.WriteLine();
                    }


                    // TODO: Implement your model
                    //publisher.Publish(outlet, "OUTPUT MESSAGE TO NEXT OUTLET");

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
            AudioProcessor audioProcessor = new AudioProcessor(publisher, "PLACEHOLDER");
            return 0;
        }
    }
}
