using System;
using System.Collections.Generic;
using System.Text;

namespace EventDetectors
{
    public static class InertiaFunctions
    {
        const double HANDLER_MAX_ANGLE = 30d;
        const double EPS = 12d;

        public static void Router(string channelName, string msg, ref StateObject state)
        {
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
        }
        public static void UpdateSpeedState(string msg, ref StateObject state)
        {
            try
            {
                ushort.TryParse(msg, out ushort parsedSpeed);
                state.CurSpeed = parsedSpeed;
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
                short.TryParse(msg.Split('|')[1], out short parsedHandler);
                state.CurHandler = (double) parsedHandler / (double) short.MaxValue * HANDLER_MAX_ANGLE;
                state.CurAccelY = GetAc(state.CurHandler, state.CurSpeed);
# if DEBUG
                Console.WriteLine(state.CurAccelY);
# endif
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static double GetAc(double turnAngle, int speed, double carLength=1.0)
        {
            
            if (Math.Abs(turnAngle) < EPS) return 0;
            double turnRad = Math.PI * turnAngle / 180d;
            double speedMeterSecond = speed * 0.277777778;
            double radius = carLength / Math.Sin(turnRad);
            // double centripetalAccel = Math.Pow(speedMeterSecond, 2d) * Math.Pow(Math.Cos(turnRad), 2d) / radius;
            Console.WriteLine($"{speedMeterSecond * Math.Tan(turnRad) / carLength}");
            // TODO: Fix Accel Function
            return centripetalAccel;
        }
    }
}
