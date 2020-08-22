using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HapticSeerDashboard
{
    class Node
    {
        private Process process;
        private string executablePath;
        private string[] inlet, outlet;
        private bool enableOutput = false;

        public Node(string executablePath, string[] inletChannels = null, string[] outletChannels = null)
        {
            this.ExecutablePath = executablePath;
            this.inlet = inletChannels;
            this.outlet = outletChannels;
        }

        public string ExecutablePath { get => executablePath; set => executablePath = value; }
        public bool EnableOutput { get => enableOutput; set => enableOutput = value; }

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
            //process.StartInfo.RedirectStandardOutput = enableOutput;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
    }
    
}
