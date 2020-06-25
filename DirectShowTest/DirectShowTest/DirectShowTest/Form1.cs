using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using System.Threading;
using System.Diagnostics;

namespace DirectShowTest
{
    public partial class Form1 : Form
    {
        FilterInfoCollection videoDevices;
        VideoCaptureDevice device;
        
        volatile int Timestamp = 0;
        static int Counter = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            device = new VideoCaptureDevice(videoDevices[0].MonikerString);
            device.NewFrame += Device_NewFrame;
            //device.VideoResolution = device.VideoCapabilities[0];
            //device.SnapshotResolution = device.SnapshotCapabilities[0];
            device.SnapshotFrame += Device_SnapshotFrame;
            device.Start();
            device.VideoSourceError += Device_VideoSourceError;
            new Thread(() => {
                Thread.CurrentThread.IsBackground = false;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (true)
                {
                    Timestamp = (int)stopwatch.ElapsedMilliseconds;
                }
            }).Start();
        }

        private void Device_VideoSourceError(object sender, AForge.Video.VideoSourceErrorEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private void Device_SnapshotFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            string fileName = $"D:\\TempOut\\{Timestamp}.png";
            Counter = 0;// ++Counter % 16;
            if (Counter == 0)
                eventArgs.Frame.Save(fileName);
        }

        private void Device_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        { 
            string fileName = $"D:\\TempOut\\{Timestamp}.png";
            Counter = ++Counter % 16;
            if (Counter == 0)
                eventArgs.Frame.Save(fileName);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = Timestamp.ToString();
        }
    }
}
