using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
namespace HLADetectors
{
    public static class FiringFunctions
    {
        private static Regex msgExp = new Regex(@"(.+)\|(.+)\|(.+)\|(.+)");
        private static Regex vecExp = new Regex(@"\{(.+), (.+), (.+)\}");
        private static int fireCount=0;

#if DEBUG
        private static Stopwatch commonStopwatch = new Stopwatch();
#endif
        

        public static void Router(string channelName, string msg, ref StateObject state)
        {
            if (!commonStopwatch.IsRunning) commonStopwatch.Start();
            if (channelName == state.openvrInlet)
            {
                UpdateInputState(msg, ref state);
            }
            else if (channelName == state.bulletInlet)
            {
                UpdateBulletState(msg, ref state);
            }
        }
        static void UpdateInputState(string inputMsg, ref StateObject state)
        {
#if DEBUG
            var start = commonStopwatch.Elapsed;
#endif
            var msg = inputMsg;
            var args = msgExp.Match(inputMsg).Groups;
            if (args[1].Value == "RightController" && args[2].Value == "Digital")
            {

                switch (args[3].Value){
                    case "Trigger":
                        state.TriggerState = args[4].Value == "Pressed" ? true : false;
                        break;
                    default:
                        break;
                }
            }
#if DEBUG
            var elapsed = commonStopwatch.Elapsed - start;
            //Console.WriteLine($"Updated XINPUT in {elapsed.TotalMilliseconds} ms");
#endif
        }
        static void UpdateBulletState(string inputMsg, ref StateObject state)
        {
#if DEBUG
            var start = commonStopwatch.Elapsed;
#endif
            ushort curBullet;
            try
            {
                ushort.TryParse(inputMsg, out curBullet);
                if (state.TriggerState)
                {
                    if (state.BulletCount > curBullet)
                    {
                        if(state.fireOutlet!=null)
                        state.publisher.Publish(state.fireOutlet, "FIRE");
# if DEBUG
                        Console.WriteLine("Fire: "+(fireCount++).ToString());
# endif
                    }
                }
                state.BulletCount = curBullet;
#if DEBUG
                //Console.WriteLine($"Bullet Count:{state.BulletCount}, Cur Bullet: {curBullet}");
                var elapsed = commonStopwatch.Elapsed-start;
                //Console.WriteLine($"Updated BULLET in {elapsed.TotalMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

        }
    }
}
