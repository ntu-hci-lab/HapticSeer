using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace BF1Detectors
{
    public static class FiringFunctions
    {
        const ushort TRIGGER_THRESHOLD = 150;
        const double COOLING_TIME_MS = 500d;
        const double EPS = 150d;
        private static int fireCount=0;

        public static void Router(string channelName, string msg, ref StateObject state)
        {
            if(channelName == state.XinputInlet)
            {
                UpdateXINPUTState(msg, ref state);
            }
            else if (channelName == state.BulletInlet)
            {
                UpdateBulletState(msg, ref state);
            } 
            else if (channelName == state.PulseInlet)
            {
                UpdatePulseState(msg, ref state);
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
                    if (state.LastTriggerEnter == null)
                    {
                        state.LastTriggerEnter = DateTime.Now;
                        state.LastTriggerExit = null;
                    }
                } 
                else
                {
                    if (state.LastTriggerExit==null && state.LastTriggerEnter != null)
                    {
                        state.LastTriggerExit = DateTime.Now;
                        state.LastTriggerEnter = null;
                    }
                    state.IsAutoFire = false;
                }
            }
        }
        static void UpdateBulletState(string inputMsg, ref StateObject state)
        {
            ushort curBullet;
            try
            {
                if (state.LastTriggerEnter.HasValue)
                {
                    var timeSpan = (DateTime.Now - state.LastTriggerEnter).Value.TotalMilliseconds;
                    ushort.TryParse(inputMsg, out curBullet);
                    if (timeSpan < EPS)
                    {
                        if (state.BulletCount > curBullet && !state.IsAutoFire)
                        {
                            if (state.FireOutlet != null)
                                state.publisher.Publish(state.FireOutlet, "FIRE");
#if DEBUG
                            Console.WriteLine("Fire: " + (fireCount++).ToString());
#endif
                        }
#if DEBUG
                        else
                        {
                            //Console.WriteLine($"Bullet Count:{state.BulletCount}, Cur Bullet: {curBullet}, Timespan: {timeSpan}");
                        }
#endif
                    }
                    state.BulletCount = curBullet;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

        }
        static void UpdatePulseState(string inputMsg, ref StateObject state)
        {
            if ( state.TriggerState > TRIGGER_THRESHOLD) {
                var timespan = (DateTime.Now - state.LastTriggerEnter).Value.Ticks 
                    / TimeSpan.TicksPerMillisecond;
                if ( timespan > EPS)
                {
                    state.IsAutoFire = true;
                    if (state.FireOutlet != null)
                        state.publisher.Publish(state.FireOutlet, "FIRE");
#if DEBUG
                    Console.WriteLine($"Auto: {state.IsAutoFire}, " + (fireCount++).ToString());
# endif
                }
            }
        }
    }
}
