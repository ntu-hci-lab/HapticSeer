using System;
using System.IO;
using System.Text;
namespace EventDetectors
{
    public static class InertiaFunctions
    {
        const double HANDLER_MAX_ANGLE = 15d;

# if LOG
        public static DateTime startTime = DateTime.Now;
        public static StreamWriter csvWriter = new StreamWriter(startTime.ToString("mm_ss_fff") + "_predict.csv")
        {
            AutoFlush = true
        };
# endif
        public static void Router(string channelName, string msg, ref StateObject state)
        {
            var start = Program.globalSW.ElapsedTicks;
            switch (channelName)
            {
                case "XINPUT":
                    int headerPos = msg.IndexOf('|');
                    if (msg.Substring(0, headerPos)== "ThumbLX")
                        UpdateNormalState(msg.Substring(headerPos+1), ref state);
                    break;
                case "SPEED":
                    UpdateSpeedState(msg, ref state);
                    break;
                default:
                    break;
            }
            //Console.WriteLine((Program.globalSW.ElapsedTicks - start) / (double) TimeSpan.TicksPerMillisecond);
        }
        public static void UpdateSpeedState(string msg, ref StateObject state)
        {
            string[] splited = msg.Split(',');
            try
            {
                ushort.TryParse(splited[0], out ushort parsedSpeed);
                if (parsedSpeed == 0) return;
                state.Speed = parsedSpeed;
                state.Angle = state.LastAngle;
                if (state.accXOutlet != null) state.publisher.Publish(state.accXOutlet, $"{state.AccelX.ToString()}");
                if (state.accYOutlet != null) state.publisher.Publish(state.accYOutlet, $"{state.AccelY.ToString()}");
# if LOG
                csvWriter.WriteLine($"{(DateTime.Now - startTime).Ticks / TimeSpan.TicksPerMillisecond},{state.AccelX},{state.AccelY}");
# endif
                Console.WriteLine(state.AccelX);
            }
            catch (Exception e) 
            {
                if (e is IndexOutOfRangeException) throw e;
                Console.WriteLine(e.Message);
            }
        }
        public static void UpdateNormalState(string msg, ref StateObject state)
        {
            
            try
            {
                short.TryParse( msg.Split('|')[1], out short parsedHandler );
                state.LastAngle = (double) parsedHandler / (double) short.MaxValue * HANDLER_MAX_ANGLE;
            } 
            catch (Exception e)
            {
                if (e is IndexOutOfRangeException) throw e;
                Console.WriteLine(e.Message);
            }
        }
    }
}
