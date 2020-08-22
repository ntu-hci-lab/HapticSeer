using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace HapticSeerDashboard
{
    class Program
    {
        public readonly static string SolutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
        public static  Node[] Nodes = new Node[3];
        public static IReadOnlyDictionary<string, string> Paths;
        static void Main(string[] args)
        {
            Paths = new Dictionary<string, string>()
            {
                {"FeatureExtract", @"ScreenCapture\ScreenCapture\bin\Debug\ScreenCapture.exe"},
                {"XInputCap",  @"XBoxInputWrapper\XBoxInputWrapper\bin\x64\Debug\netcoreapp3.1\XBoxInputWrapper.exe"},
                {"InertiaDetect", @"InertiaDetector\bin\Debug\netcoreapp3.1\InertiaDetector.exe"}
            };

            Console.WriteLine(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory));
            Nodes[0] = new Extractor(Path.Combine(SolutionRoot, Paths["FeatureExtract"]), "PC2", 
                new string[] {"SPEED"}
            );
            Nodes[1] = new Node(Path.Combine(SolutionRoot, Paths["XInputCap"]))
            {
                EnableOutput = true
            };
            Nodes[2] = new Node(Path.Combine(SolutionRoot, Paths["InertiaDetect"]),
                new string[] { "SPEED", "XINPUT" },
                new string[] { "AccelX", "AccelY" })
            {
                EnableOutput = true
            };
            

            Console.WriteLine(Nodes[0].ExecutablePath);
            for (int i = 0; i < Nodes.Length; i++) 
            {
                Nodes[i].Launch();
            }

            
            _ = Console.ReadKey();
        }
    }
}
