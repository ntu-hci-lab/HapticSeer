using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatencyLogger
{
    public class LatencyLoggerBase
    {
        public Dictionary<string, StreamWriter> processTimeLoggers = new Dictionary<string, StreamWriter>();
        public DateTimeOffset startTimestamp;
        public readonly static string SolutionRoot = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
        public static Stopwatch sw;

        public LatencyLoggerBase(string[] loggerNames, string gamePostfix)
        {
            startTimestamp = DateTimeOffset.UtcNow;
            if (sw == null) 
            {
                sw = new Stopwatch();
                sw.Start();       
            }
            foreach (string name in loggerNames)
            {
                processTimeLoggers.Add(name, new StreamWriter(Path.Combine(SolutionRoot,
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
