using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
namespace HapticSeerDashboard
{
    class Program
    {
        public readonly static string SolutionRoot = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
        public static List<NodeBaseInstance> nodes = new List<NodeBaseInstance>();
        static void Main(string[] args)
        {
            using (StreamReader f = File.OpenText(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "sampleConfig2.json")))
            {
                Console.WriteLine($"Solution Root is: {SolutionRoot}");
                JsonSerializer serializer = new JsonSerializer();
                ConfigSchema schema = (ConfigSchema) serializer.Deserialize(f, typeof(ConfigSchema));

                foreach (var detector in schema.eventDetectors)
                {
                    nodes.Add(new NodeBaseInstance(Path.Combine(SolutionRoot,
                        schema.Paths[detector.Type]), detector.Inlets, detector.Outlets, detector.Options));
                }
                foreach (var extractor in schema.extractorSets)
                {
                    nodes.Add(new ExtractorSet(Path.Combine(SolutionRoot,
                        schema.Paths["FeatureExtract"]), extractor.Type, extractor.Outlets, extractor.Options));
                }
                foreach (var capturer in schema.rawCapturers)
                {
                    nodes.Add(new RawCapturer(Path.Combine(SolutionRoot,
                        schema.Paths[capturer.Type]), capturer.Outlets, capturer.Options));
                }

                foreach (var node in nodes)
                {
                    Console.WriteLine(node.ExecutablePath);
                    node.Launch();
                }
                _ = Console.ReadKey();
            }
            
        }
    }
}
