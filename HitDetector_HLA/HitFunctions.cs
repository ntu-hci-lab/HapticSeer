#define LOG
using System;
using System.Diagnostics;
using static LatencyLogger.LatencyLoggerBase;

namespace HLADetectors
{
    public static class HitFunctions
    {
        const ushort EPS = 2;
        const double HP_UPPERBOUND = 70;
        const double HP_SENSITIVITY = 4;
#if DEBUG
        private static Stopwatch commonStopwatch = new Stopwatch();
#endif
        public static void Router(string channelName, string msg, ref StateObject state)
        {
            if (!commonStopwatch.IsRunning) commonStopwatch.Start();
            if (channelName == state.bloodInlet)
            {
                UpdateHPState(msg, ref state);
            }
        }
        static void UpdateHPState(string inputMsg, ref StateObject state)
        {
            var startMs = GetElapsedMillseconds();
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
                        var bloodLoss = state.RealHP - roundedCurHP;
                        if (bloodLoss > HP_SENSITIVITY)
                        {
                            state.LastBloodLossSignal = DateTime.Now;
                            state.RealHP = roundedCurHP;
                            Program.loggers.processTimeLoggers["hit_detector"].WriteLineAsync(
                                $"{startMs},{GetElapsedMillseconds()},1"
                            );
                            if (state.hitOutlet!=null)
                                state.publisher.Publish(state.hitOutlet, bloodLoss.ToString());
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
