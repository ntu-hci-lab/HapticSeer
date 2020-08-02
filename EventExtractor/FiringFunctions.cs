using System;
using System.Collections.Generic;
using System.Text;

namespace EventDetectors
{
    public static class FiringFunctions
    {
        private static int fireCount=0;
        const ushort TRIGGER_THRESHOLD = 180;
        const double COOLING_TIME_MS = 500d;
        const double EPS = 150d;


        public static void Router(string channelName, string msg, ref WeaponState state)
        {
            switch (channelName)
            {
                case "XINPUT":
                    UpdateXINPUTState(msg, ref state);
                    break;
                case "BULLET":
                    UpdateBulletState(msg, ref state);
                    break;
                case "IMPULSE":
                    UpdateImpluseState(msg, ref state);
                    break;
                default:
                    break;
            }
        }
        static void UpdateXINPUTState(string inputMsg, ref WeaponState state)
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
                    if ((DateTime.Now - state.LastTriggerExit).TotalMilliseconds > COOLING_TIME_MS)
                    {
                        state.LastTriggerEnter = DateTime.Now;
                    }
                    state.LastTriggerExit = DateTime.Now;
                } else
                {
                    state.IsAutoFire = false;
                }
            }
        }
        static void UpdateBulletState(string inputMsg, ref WeaponState state)
        {
            ushort curBullet;
            try
            {
                var timeSpan = (DateTime.Now - state.LastTriggerExit).TotalMilliseconds;
                ushort.TryParse(inputMsg, out curBullet);
                if (timeSpan < EPS)
                {
                    if (state.BulletCount > curBullet && !state.IsAutoFire)
                    {
                        state.publisher.Publish("FIRING", "FIRE");
# if DEBUG
                        Console.WriteLine("Fire: "+(fireCount++).ToString());
# endif
                    }
# if DEBUG
                    else
                    {
                        //Console.WriteLine($"Bullet Count:{state.BulletCount}, Cur Bullet: {curBullet}, Timespan: {timeSpan}");
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
        static void UpdateImpluseState(string inputMsg, ref WeaponState state)
        {
            if( state.TriggerState > TRIGGER_THRESHOLD)
            {
# if DEBUG
                Console.WriteLine($"Auto: {state.IsAutoFire}, {(DateTime.Now - state.LastTriggerEnter).Ticks}");
# endif
                if ((DateTime.Now - state.LastTriggerEnter).Ticks > 0)
                {
                    state.IsAutoFire = true;
# if DEBUG
                    Console.WriteLine("Fire: "+(fireCount++).ToString());
# endif
                }
            }
        }
    }
}
