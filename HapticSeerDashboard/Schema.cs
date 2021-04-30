using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace HapticSeerDashboard
{
    public class Schema
    {
        abstract public class Components
        {
            private static string solutionRoot;
            private string name, preset;
            private string[] inlets, outlets;
            private bool enableOutput = false;
            private Process process;
            protected static Dictionary<string, string> paths;
            [JsonExtensionData]
            protected IDictionary<string, JToken> _additionalData;


            public string ExecutablePath { get => GetPath(Name, Preset); set { } }
            public string Name { get => name; set => name = value; }
            public string Preset { get => preset; set => preset = value; }
            public string[] Inlets { get => inlets; set => inlets = value; }
            public string[] Outlets { get => outlets; set => outlets = value; }
            public bool EnableOutput { get => enableOutput; set => enableOutput = value; }
            public Dictionary<string, string> Options { get; set; }
            public string SolutionRoot { get => solutionRoot; set => solutionRoot = value; }

            public Components()
            {
                _additionalData = new Dictionary<string, JToken>();
            }


            virtual protected string GetPath(string type, string subtype)
            {
                string exePath = null;
                if (paths == null) throw new InvalidOperationException("You must load paths first!");
                if (subtype != null)
                {
                    paths.TryGetValue($"{type}_{subtype}", out exePath);
                }
                else
                {
                    paths.TryGetValue($"{type}", out exePath);
                }
                return exePath;
            }
            static public void LoadPath(Dictionary<string, string> paths, String solutionRoot)
            {
                if (Components.paths == null) 
                {
                    Components.paths = paths;
                    Components.solutionRoot = solutionRoot;
                } 
            }
            virtual public void Launch()
            {
                if (ExecutablePath == null) throw new MissingFieldException(message: "Executable path not assigned!");
                process = new Process();
                process.StartInfo.FileName = Path.Combine(
                    solutionRoot,
                    ExecutablePath
                    );
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                if (inlets != null)
                {
                    for (int i = 0; i < inlets.Length; i++) process.StartInfo.ArgumentList.Add(inlets[i]);
                }
                if (outlets != null)
                {
                    for (int i = 0; i < outlets.Length; i++) process.StartInfo.ArgumentList.Add(outlets[i]);
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

        public class ExtractorSet : Components
        {
            public ExtractorSet()
            {

            }
            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                if(Preset != null)
                    Inlets = new string[] { Preset };
            }
            protected override string GetPath(string type, string subtype)
            {
                if (paths == null) throw new InvalidOperationException("You must load paths first!");
                paths.TryGetValue($"{type}", out string exePath);
                return exePath;
            }
        }

        public class RawCapturer : Components 
        {
            public RawCapturer() { }
            protected override string GetPath(string type, string subtype)
            {
                if (paths == null) throw new InvalidOperationException("You must load paths first!");
                paths.TryGetValue($"{type}", out string exePath);
                return exePath;
            }
        }
        
        public class EventDetector : Components
        {
            public EventDetector() { }
        }
        public List<ExtractorSet> extractorSets = new List<ExtractorSet>();
        public List<RawCapturer> rawCapturers = new List<RawCapturer>();
        public List<EventDetector> eventDetectors = new List<EventDetector>();
    }
}
