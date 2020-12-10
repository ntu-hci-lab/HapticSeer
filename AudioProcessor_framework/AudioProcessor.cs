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

// for pitch trcaker
using System.Text;
using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
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

        //// pitch tracker
        //private PitchTracker m_pitchTracker;
        //private DispatcherTimer m_timer;
        //private float[] m_audioBuffer;
        //private int m_timeInterval;
        //private float m_sampleRate;
        //private double m_curWaveAngle;

        //private readonly float m_minPitch = 55.0f;
        //private readonly float m_maxPitch = 1500.0f;

        //   // UI value
        //    double m_sliderAmplitude = -20;
        //    double m_sliderPitch = 200;

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

        //void DetectPitch(List<float[]> pcmBuffers)
        //{
        //    m_sampleRate = 44100.0f;
        //    m_timeInterval = 100;  // 100ms

        //    //InitializeComponent();

        //    this.GeneratorPitch = 1000.0f;
        //    this.GeneratorAmplitude = 0.1f;


        //    m_pitchTracker = new PitchTracker();
        //    m_pitchTracker.SampleRate = m_sampleRate;
        //    //m_pitchTracker.PitchDetected += OnPitchDetected;

        //    m_audioBuffer = new float[(int)Math.Round(m_sampleRate * m_timeInterval / 1000.0)];


        //    // Calculate the midi here

        //    m_timer = new DispatcherTimer();
        //    m_timer.Interval = TimeSpan.FromMilliseconds(m_timeInterval);
        //    m_timer.Tick += OnTimerTick;
        //    m_timer.Start();

        //    //Console.Write("Hz:" + GeneratorPitch);
        //    //Console.Write("MIDI:" + )

        //    //UpdateDisplay(pcmBuffers);
        //}

        ///// Detect Pitch Start
        //private float GeneratorPitch
        //{
        //    get
        //    {
        //        var sliderRatio = m_sliderPitch / 15000;
        //        var maxVal = Math.Log10(m_maxPitch / m_minPitch);
        //        var pitch = (float)Math.Pow(10.0, sliderRatio * maxVal) * m_minPitch;

        //        return pitch;
        //    }

        //    set
        //    {
        //        if (value <= m_minPitch)
        //        {
        //            m_sliderPitch = 55;
        //        }
        //        else if (value >= m_maxPitch)
        //        {
        //            m_sliderPitch = 15000;
        //        }
        //        else
        //        {
        //            var maxVal = Math.Log10(m_maxPitch / m_minPitch);
        //            var curVal = Math.Log10(value / m_minPitch);
        //            var slider = 15000 * curVal / maxVal;

        //            m_sliderPitch = slider;
        //        }
        //    }
        //}

        //private float GeneratorAmplitude
        //{
        //    get { return (float)Math.Pow(10.0, m_sliderAmplitude / 20.0); }
        //    set { m_sliderAmplitude = 20.0 * Math.Log10(value); }
        //}

        //private void OnTimerTick(object sender, EventArgs e)
        //{
        //    m_curWaveAngle = PitchDsp.CreateSineWave(m_audioBuffer, m_audioBuffer.Length,
        //        m_sampleRate, this.GeneratorPitch, this.GeneratorAmplitude, m_curWaveAngle);

        //    m_pitchTracker.ProcessBuffer(m_audioBuffer);

        //}

        //private void UpdateDisplay(List<float[]> pcmBuffers)
        //{
        //    ///// Show the generator pitch
        //    var curPitch = this.GeneratorPitch;

        //    Console.WriteLine("Generate pitch:" + curPitch + " Hz");
        //    // Show the generator amplitude
        //    Console.WriteLine("Generate Amp:" + m_sliderAmplitude);

        //    Console.WriteLine(m_pitchTracker.CurrentPitchRecord.Pitch + " PITCH");
        //    ////// Show the detector results
        //    //var curPitchRecord = m_pitchTracker.CurrentPitchRecord;
        //    var curPitchRecord = pcmBuffers[0];


        //    string m_lblDetectorPitch;
        //    string m_lblDetectorMidiNote;
        //    int m_lblDetectorMidiCents;
        //    string m_lblDetectorPitchError;

        //    if (curPitchRecord.Pitch > 1.0f)
        //    {

        //        m_lblDetectorPitch = curPitchRecord.Pitch.ToString();
        //        m_lblDetectorMidiNote = PitchDsp.GetNoteName(curPitchRecord.MidiNote, true, true);
        //        m_lblDetectorMidiCents = curPitchRecord.MidiCents;

        //        var diffPercent = 100.0 - (100.0f * this.GeneratorPitch / curPitchRecord.Pitch);



        //        if (diffPercent >= 0.0f)
        //            m_lblDetectorPitchError = "+" + diffPercent.ToString();
        //        else
        //            m_lblDetectorPitchError = diffPercent.ToString();
        //    }
        //    else
        //    {
        //        m_lblDetectorPitch = "--";
        //        m_lblDetectorPitchError = "--";
        //        m_lblDetectorMidiNote = "--";
        //        m_lblDetectorMidiCents = 0;
        //    }

        //    Console.WriteLine(m_lblDetectorPitch + "  Hz");
        //    Console.WriteLine(m_lblDetectorMidiNote + "(" + m_lblDetectorMidiCents + ")");
        //    Console.WriteLine("Error:" + m_lblDetectorPitchError);

        //}
        ///// Detect Pitch End




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
