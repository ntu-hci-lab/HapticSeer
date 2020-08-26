using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using static HapticSeerDashboard.Schema;
namespace HapticSeerDashboard
{
    class Program
    {
        public readonly static string SolutionRoot = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
        public static Dictionary<string, string> paths =
            JsonConvert.DeserializeObject<Dictionary<string, string>>
            (
                File.ReadAllText
                (
                    Path.Combine
                    (
                        AppDomain.CurrentDomain.BaseDirectory,
                        "path.json"
                    )
                )
            );

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                throw new MissingFieldException("No Configuration File Provided!");
            }
            using StreamReader f = File.OpenText(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                args[0]));
            Console.WriteLine($"Solution Root is: {SolutionRoot}");
            JsonSerializer serializer = new JsonSerializer();
            Components.LoadPath(paths, SolutionRoot);
            Schema schema = (Schema)serializer.Deserialize(f, typeof(Schema));

            foreach (var cap in schema.rawCapturers)
            {
                Console.WriteLine(cap.ExecutablePath);
                cap.Launch();
            }
            foreach (var ext in schema.extractorSets)
            {
                Console.WriteLine(ext.ExecutablePath);
                ext.Launch();
            }
            foreach (var dect in schema.eventDetectors)
            {
                Console.WriteLine(dect.ExecutablePath);
                dect.Launch();
            }
            _ = Console.ReadKey();

        }
    }
}
