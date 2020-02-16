using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WPFCaptureSample.AudioRecorder
{
    public class AudioLoopback
    {
        IWaveIn waveIn;
        WaveFileWriter file = null;
        uint waveInIndex;
        public AudioLoopback()
        {
            waveIn = new WasapiLoopbackCapture(WasapiLoopbackCapture.GetDefaultLoopbackCaptureDevice());
            if (waveIn.WaveFormat.Channels < 4)
                MessageBox.Show("The channels of speaker < 4.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);
            waveIn.StartRecording();
        }
        public void StartRecord(string FilePath)
        {
            waveInIndex = 0;
            file = new WaveFileWriter(FilePath + "Sound.wav", waveIn.WaveFormat);
        }
        public async void StopRecord()
        {
            uint LastWaveInIndex = waveInIndex;
            while (LastWaveInIndex == waveInIndex)
                await Task.Delay(1);
            file.Flush();
            file.Dispose();
            file = null;
        }
        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (file != null)
            {
                ++waveInIndex;
                file.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
        }
    }
}

