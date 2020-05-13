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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            var a = new VideoCaptureDevice(videoDevices[0].MonikerString);
        }
    }
}
