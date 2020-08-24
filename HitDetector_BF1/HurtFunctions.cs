using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BF1Detectors
{
    public static class HurtFunctions
    {
        const ushort HURT_EPS = 1;
        const ushort HIT_EPS = 50;
        public static void Router(string channelName, string msg, ref StateObject state)
        {
            if (channelName == state.BloodInlet)
            {
                UpdateHPState(msg, ref state);
            }
            else if (channelName == state.HitInlet)
            {
                UpdateHitState(msg, ref state);
            }
        }
        static void UpdateHPState(string inputMsg, ref StateObject state)
        {
            double curHP;
            byte roundedCurHP;
            try
            {
                double.TryParse(inputMsg, out curHP);
                roundedCurHP = (byte)Math.Round(curHP * 100);
                double bloodLoss = state.RealHP - roundedCurHP;
                var now = DateTime.Now;
                if(roundedCurHP == 0)
                {
                    state.RealHP = 100;
                }
                else if (roundedCurHP > 10)
                {
                    if (bloodLoss > 1)
                    {
                        state.LastBloodLossSignal = now;
                        state.RealHP = roundedCurHP;
                        if ((now - state.LastBloodLossSignal).TotalMilliseconds < HURT_EPS &&
                            (now - state.LastHitSignal).TotalMilliseconds       > HIT_EPS)
                        {
                            state.LastHitSignal = now;
                            if (state.IncomingOutlet != null)
                                state.publisher.Publish(state.IncomingOutlet, $"{bloodLoss},{state.LastHitAngle.ToString()}");
#if DEBUG
                            Console.WriteLine($"INCOMING, {bloodLoss}|{state.LastHitAngle.ToString()}");
#endif
                        }
                    }
                    else if (bloodLoss > -10)
                    {
                        state.RealHP = roundedCurHP;
                    }
                }
                
#if DEBUG
                Console.WriteLine($"Real: {state.RealHP}, Reading: {roundedCurHP}");
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        static void UpdateHitState(string inputMsg, ref StateObject state)
        {
            double angle;
            try
            {
                double.TryParse(inputMsg, out angle);
                state.LastHitAngle = angle;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
