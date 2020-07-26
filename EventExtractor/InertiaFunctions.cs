using System;
using System.Collections.Generic;
using System.Text;

namespace EventDetectors
{
    static class InertiaFunctions
    {
        public static double FPS = 30;
        public static void Parser(string msg, LinkedList<ushort> curSpeed)
        {
            var sep = msg.IndexOf('|');
            var type = msg.Substring(0, sep);
            switch (type)
            {
                case "SMOOTHED":
                    curSpeed.AddLast(ushort.Parse(msg.Substring(sep + 1)));
                    curSpeed.RemoveFirst();
                    Console.WriteLine((double)(curSpeed.Last.Value - curSpeed.First.Value) /  (FPS / 60));
                    break;
                case "ThumbLX":
                    break;
                default:
                    break;
            }
        }
    }
}
