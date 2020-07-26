using System;
using System.Collections.Generic;
using System.Text;

namespace EventDetectors
{
    public static class FiringFunctions
    {
        public static void Router(string channelName, string msg, ref byte triggerState)
        {
            switch (channelName)
            {
                case "XINPUT":
                    XINPUTStateChange(msg, ref triggerState);
                    break;
                case "BULLET":
                    break;
                default:
                    break;
            }
        }
        static void XINPUTStateChange(string inputMsg, ref byte triggerState)
        {
            var msg = inputMsg;
            var sep = msg.IndexOf('|');
            var header = msg.Substring(0, sep);
            var args = msg.Substring(sep).Split('|');
            if (msg.Substring(0, sep) == "RightTrigger")
            {
                triggerState = byte.Parse(args[2]);
            }
        }
    }
}
