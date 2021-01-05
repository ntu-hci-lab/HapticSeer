using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using RedisEndpoint;
namespace HLADetectors
{
    class Program
    {
        const string URL = "localhost";
        const ushort PORT = 6380;


        static int Main(string[] args)
        {
            HitDetector h = new HitDetector(URL, PORT, args[0], args[1]);
            _ = Console.ReadKey();
            return 0;
        }
    }
}