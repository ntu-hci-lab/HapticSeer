using System;
using System.Collections.Generic;
using System.Text;

namespace EventDetectors
{
    public static class FiringFunctions
    {
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
            }
        }
        static void UpdateBulletState(string inputMsg, ref StateObject state)
        {
            ushort curBullet;
            try
            {
                ushort.TryParse(inputMsg, out curBullet);
                if (state.TriggerState > 80)
                {

                    if (state.BulletCount > curBullet)
                    {
                        Console.WriteLine("Fire");
                    }
                    state.BulletCount = curBullet;
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
