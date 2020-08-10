using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EventDetectors
{
    public static class HurtFunctions
    {
        const ushort EPS = 2;
        const double HP_UPPERBOUND = 70;
        const double HP_SENSITIVITY = 4;
#if DEBUG
        private static Stopwatch commonStopwatch = new Stopwatch();
#endif
        public static void Router(string channelName, string msg, ref HealthState state)
        {
            if (!commonStopwatch.IsRunning) commonStopwatch.Start();
            switch (channelName)
            {
                case "BLOOD":
                    UpdateHPState(msg, ref state);
                    break;
                default:
                    break;
            }
        }
        static void UpdateHPState(string inputMsg, ref HealthState state)
        {
            var start = commonStopwatch.Elapsed;
            double curHP;
            byte roundedCurHP;
            try
            {
                double.TryParse(inputMsg, out curHP);
                roundedCurHP = (byte) Math.Round(curHP * 100);

                if (roundedCurHP <= HP_UPPERBOUND)
                {
                    if (roundedCurHP != 0)
                    {
                        if (state.RealHP - roundedCurHP > HP_SENSITIVITY)
                        {
                            state.LastBloodLossSignal = DateTime.Now;
                            state.RealHP = roundedCurHP;
                            Console.WriteLine("HIT");
                        }
                        else if (roundedCurHP - state.RealHP > 10)
                        {
                            if(state.LastHPBurst != null)
                            {
                                if ((DateTime.Now - state.LastHPBurst).Value.TotalSeconds > 1)
                                {
                                    Console.WriteLine("HEAL");
                                    state.RealHP = roundedCurHP;
                                    state.LastHPBurst = null;
                                } 
                            } 
                            else
                            {
                                state.LastHPBurst = DateTime.Now;
                            }
                            
                        }
                    }
                    else
                    {
                        state.RealHP = 100;
                    }
                }
#if DEBUG
                Console.WriteLine($"Real: {state.RealHP}, Reading: {roundedCurHP}");
                var elapsed = commonStopwatch.Elapsed - start;
                //Console.WriteLine($"Updated HP in {elapsed.TotalMilliseconds} ms");
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
