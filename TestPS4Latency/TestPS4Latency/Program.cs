using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Drawing;
using AForge.Video.DirectShow;
namespace TestPS4Latency
{
    class Program
    {
        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
        static ulong StartRecordTimeStamp = 0;

        static FilterInfoCollection videoDevices;
        static VideoCaptureDevice device;
        static int Counter = 0;

        private static void Device_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Counter = ++Counter % 64;

            if (Counter == 0)
            {
                ulong Timestamp = GetTickCount64();
                ulong delta = Timestamp - StartRecordTimeStamp;
                string fileName = $"D:\\TempOut\\{delta}.png";
                eventArgs.Frame.Save(fileName);
            }
        }
        static void Test()
        {
            string WebUrl = "http://localhost:2222/";// "http://192.168.50.242:2222/";
            HttpListener server = new HttpListener();
            server.Prefixes.Add(WebUrl);
            //server.Prefixes.Add("http://localhost:2222/");

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
                        builder = new StringBuilder($"<!DOCTYPE html><html><head><script>function A(){{fetch('{WebUrl}timestamp').then(function(response) {{return response.text()}}).then(function(html) {{document.body.innerText = html;}}).catch(function(err) {{console.log('Failed to fetch page: ', err);}});}}setInterval(\"A()\", 1);</script></head><body></body></html>");
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

            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            device = new VideoCaptureDevice(videoDevices[0].MonikerString);
            device.NewFrame += Device_NewFrame;
            device.Start();

            StartRecordTimeStamp = GetTickCount64();
            new Thread(() =>
            {
                Test();
            }).Start();
            while (true)
                Thread.Sleep(100);
        }
    }
}
