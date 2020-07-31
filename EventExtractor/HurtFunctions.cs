using System;
using System.Collections.Generic;
using System.Text;

namespace EventDetectors
{
    public static class HurtFunctions
    {
        const ushort EPS = 2;

        public static void Router(string channelName, string msg, ref HealthState state)
        {
            switch (channelName)
            {
                case "BLOOD":
                    UpdateHPState(msg, ref state);
                    break;
                case "HIT":
                    UpdateHitState(msg, ref state);
                    break;
                default:
                    break;
            }
        }
        static void UpdateHPState(string inputMsg, ref HealthState state)
        {
            double curHP;
            byte roundedCurHP;
            try
            {
                double.TryParse(inputMsg, out curHP);
                roundedCurHP = (byte) Math.Round(curHP*100);

                if (roundedCurHP != 0)
                {
                    if (roundedCurHP < state.RealHP)
                    {
                        state.LastBloodLossSignal = DateTime.Now;
                        state.RealHP = roundedCurHP;
                    }
                    else if (roundedCurHP - state.RealHP < 10)
                    {
                        state.RealHP = roundedCurHP;
                    }
                } else
                {
                    state.RealHP = 100;
                }
#if DEBUG
                Console.WriteLine($"Real: {state.RealHP}, Reading: {roundedCurHP}");
#endif
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static void UpdateHitState(string inputMsg, ref HealthState state)
        {
            double angle;
            try
            {
                if ((DateTime.Now-state.LastBloodLossSignal).TotalMilliseconds < EPS)
                {
                    double.TryParse(inputMsg, out angle);
# if DEBUG
                    Console.WriteLine($"Hit: {angle}");
#endif
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
