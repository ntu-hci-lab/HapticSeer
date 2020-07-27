using System;
using System.Collections.Generic;
using System.Text;

namespace EventDetectors
{
    public static class FiringFunctions
    {
        const ushort TRIGGER_THRESHOLD = 180;
        const double EPS = 150d; 

        public static void Router(string channelName, string msg, ref StateObject state)
        {
            switch (channelName)
            {
                case "XINPUT":
                    UpdateXINPUTState(msg, ref state);
                    break;
                case "BULLET":
                    UpdateBulletState(msg, ref state);
                    break;
                default:
                    break;
            }
        }
        static void UpdateXINPUTState(string inputMsg, ref StateObject state)
        {
            var msg = inputMsg;
            var sep = msg.IndexOf('|');
            var header = msg.Substring(0, sep);
            var args = msg.Substring(sep).Split('|');
            if (msg.Substring(0, sep) == "RightTrigger")
            {
                state.TriggerState = byte.Parse(args[2]);
                if (state.TriggerState > TRIGGER_THRESHOLD)
                {
                    state.LastTriggerExit = DateTime.Now;
                }
            }
        }
        static void UpdateBulletState(string inputMsg, ref StateObject state)
        {
            ushort curBullet;
            try
            {
                var timeSpan = (DateTime.Now - state.LastTriggerExit).TotalMilliseconds;
                ushort.TryParse(inputMsg, out curBullet);
                if (timeSpan < EPS)
                {
                    if (state.BulletCount > curBullet )
                    {
                        state.publisher.Publish("FIRING", "FIRE");
                        Console.WriteLine("Fire");
                    }
# if DEBUG
                    else
                    {
                        Console.WriteLine($"Bullet Count:{state.BulletCount}, Cur Bullet: {curBullet}, Timespan: {timeSpan}");
                    }
# endif
                }
                state.BulletCount = curBullet;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            
        }
    }
}
