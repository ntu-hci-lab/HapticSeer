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

namespace DirectShowTest
{
    public partial class Form1 : Form
    {
        FilterInfoCollection videoDevices;
        VideoCaptureDevice device;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            device = new VideoCaptureDevice(videoDevices[0].MonikerString);
            device.NewFrame += Device_NewFrame;
        }

        private void Device_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            //eventArgs.Frame.Save(@"D:\File.png");
        }
    }
}
