using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HapticSeerDashboard
{
    class NodeBaseInstance
    {
        private Process process;
        private string executablePath;
        private string[] inlet, outlet;
        private bool enableOutput = false;

        public Dictionary<string, string> Options = new Dictionary<string, string>();
        public bool EnableOutput { get => enableOutput; set => enableOutput = value; }
        public string ExecutablePath { get => executablePath;}

        public NodeBaseInstance(string executablePath,
                                string[] inletChannels = null,
                                string[] outletChannels = null,
                                Dictionary<string, string> options = null)
        {
            this.Options = options;
            this.executablePath = executablePath;
            this.inlet = inletChannels;
            this.outlet = outletChannels;
        }
        public void Launch()
        {
            if (ExecutablePath == null) throw new MissingFieldException(message: "Executable path not assigned!");
            process = new Process();
            process.StartInfo.FileName = ExecutablePath;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            if(inlet!= null)
            {
                for (int i=0;i<inlet.Length;i++) process.StartInfo.ArgumentList.Add(inlet[i]);
            }
            if (outlet != null)
            {
                for (int i = 0; i < outlet.Length; i++) process.StartInfo.ArgumentList.Add(outlet[i]);
            }
            if (Options != null)
            {
                process.StartInfo.UseShellExecute = Convert.ToBoolean(
                    Options.GetValueOrDefault("UseShellExecute", "False"));
                process.StartInfo.RedirectStandardOutput = Convert.ToBoolean(
                    Options.GetValueOrDefault("RedirectStandardOutput", "False"));
            }
            process.Start();
        }
    }
    class ExtractorSet : NodeBaseInstance
    {
        public ExtractorSet(
            string executablePath,
            string gameName,
            string[] outletChannels = null,
            Dictionary<string, string> options = null): 
            base(executablePath,
                 new string[] { gameName },
                 outletChannels,
                 options)
        { 

        }
    }
    class RawCapturer : NodeBaseInstance
    {
        public RawCapturer(
            string executablePath,
            string[] outletChannels = null,
            Dictionary<string, string> options = null): 
            base(executablePath,
                 null,
                 outletChannels,
                 options)
        {

        }
    }
}
