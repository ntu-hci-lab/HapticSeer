﻿using System;
namespace EventDetectors
{
    public static class InertiaFunctions
    {
        const double HANDLER_MAX_ANGLE = 15d;

        public static void Router(string channelName, string msg, ref StateObject state)
        {
            var start = Program.globalSW.ElapsedTicks;
            switch (channelName)
            {
                case "XINPUT":
                    int headerPos = msg.IndexOf('|');
                    if(msg.Substring(0, headerPos)== "ThumbLX")
                        UpdateNormalState(msg.Substring(headerPos+1), ref state);
                    break;
                case "SPEED":
                    UpdateSpeedState(msg, ref state);
                    break;
                default:
                    break;
            }
            Console.WriteLine((Program.globalSW.ElapsedTicks - start) / (double) TimeSpan.TicksPerMillisecond);
        }
        public static void UpdateSpeedState(string msg, ref StateObject state)
        {
            try
            {
                ushort.TryParse(msg, out ushort parsedSpeed);
                state.Speed = parsedSpeed;
                state.Angle = state.LastAngle;
                //Console.WriteLine($"AccelY: {state.AccelY}");
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void UpdateNormalState(string msg, ref StateObject state)
        {
            
            try
            {
                short.TryParse( msg.Split('|')[1], out short parsedHandler );
                state.LastAngle = (double) parsedHandler / (double) short.MaxValue * HANDLER_MAX_ANGLE;
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
