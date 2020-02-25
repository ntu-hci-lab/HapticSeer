﻿using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using AForge.Video.DirectShow;
using System.Drawing.Bitmap;
namespace TestPS4Latency
{
    class Program
    {
        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
        static Mat frame = new Mat();
        static VideoCapture cap = new VideoCapture();
        static ulong StartRecordTimeStamp = 0;
        static FilterInfoCollection USB_Webcams = null;
        static VideoCaptureDevice Cam = null;

        static void ProcessFrame(object sender, EventArgs e)
        {
            if (cap != null && cap.Ptr != IntPtr.Zero)
            {
                cap.Retrieve(frame, 0);
                if (StartRecordTimeStamp > 0)
                {
                    ulong Timestamp = GetTickCount64();
                    ulong delta = Timestamp - StartRecordTimeStamp;
                    frame.Save(delta.ToString() + ".png");
                }

            }
        }
        static void ForCaptureCard()
        {
            USB_Webcams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (USB_Webcams.Count > 0)  // The quantity of WebCam must be more than 0.
            {
                Cam = new VideoCaptureDevice(USB_Webcams[0].MonikerString);
                Cam.NewFrame += Cam_NewFrame;
                Cam.Start();
                Thread.Sleep(100);
            }

        }

        private static void Cam_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            if (StartRecordTimeStamp == 0)
                return;
            ulong Timedelta = GetTickCount64() - StartRecordTimeStamp;
            eventArgs.Frame.Save(Timedelta.ToString() + ".png");
        }

        static void Test()
        {
            HttpListener server = new HttpListener();
            //server.Prefixes.Add("http://192.168.1.103:2222/");
            server.Prefixes.Add("http://localhost:2222/");

            server.Start();

            Console.WriteLine("Listening...");

            while (true)
            {
                HttpListenerContext context = server.GetContext();
                HttpListenerResponse response = context.Response;

                string page = Directory.GetCurrentDirectory() + context.Request.Url.LocalPath;
                Console.WriteLine(context.Request.Url.LocalPath);
                if (page == string.Empty)
                    page = "index.html";
                ulong Timedelta = GetTickCount64() - StartRecordTimeStamp;
                StringBuilder builder;
                switch (context.Request.Url.LocalPath)
                {
                    case "/timestamp":
                        builder = new StringBuilder(Timedelta.ToString());
                        break;
                    default:
                        Console.WriteLine(context.Request.QueryString.Get("test"));
                        builder = new StringBuilder("<!DOCTYPE html><html><head><script>function A(){fetch('http://localhost:2222/timestamp').then(function(response) {return response.text()}).then(function(html) {document.body.innerText = html;}).catch(function(err) {console.log('Failed to fetch page: ', err);});}setInterval(\"A()\", 1);</script></head><body></body></html>");
                        break;
                }

                string something = builder.ToString();
                byte[] buffer = Encoding.UTF8.GetBytes(something);
                response.ContentLength64 = buffer.Length;
                Stream st = response.OutputStream;
                st.Write(buffer, 0, buffer.Length);

                context.Response.Close();
            }

        }
        static void Main(string[] args)
        {
            cap.ImageGrabbed += new EventHandler(ProcessFrame);
            cap.Start();
            Thread.Sleep(100);
            StartRecordTimeStamp = GetTickCount64();
            new Thread(() =>
            {
                Test();
            }).Start();
            while (true)
                Thread.Sleep(1); ;
        }
    }
}