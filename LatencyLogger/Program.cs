using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace EvaluationLogger
{
    public class Base
    {
        public Dictionary<string, StreamWriter> loggerDict = new Dictionary<string, StreamWriter>();
        public DateTimeOffset startTimestamp;
        public readonly static string SolutionRoot = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
        public static Stopwatch sw;

        public Base(string[] loggerNames, string gamePostfix)
        {
            startTimestamp = DateTimeOffset.UtcNow;
            if (sw == null) 
            {
                sw = new Stopwatch();
                sw.Start();       
            }
            foreach (string name in loggerNames)
            {
                loggerDict.Add(name, new StreamWriter(Path.Combine(SolutionRoot,
                $"{name}_{gamePostfix}_{startTimestamp.ToUnixTimeMilliseconds()}.csv"))
                {
                    AutoFlush = true
                });
            }
        }
        public static double GetElapsedMillseconds()
        {
            return (double) sw.ElapsedTicks / Stopwatch.Frequency * 1000;
        }
        public static void Main()
        {

        }
    }
}
