using System;
using System.Runtime.InteropServices;
using System.Security;

namespace XBoxInputWrapper
{
    partial class Program
    {
        public enum EventType
        {
            ThumbLX,
            ThumbLY,
            ThumbRX,
            ThumbRY,
            LeftTrigger,
            RightTrigger,
            Buttons
        }
        struct XInputGamepad
        {
            public ushort Buttons;
            public byte LeftTrigger;
            public byte RightTrigger;
            public short ThumbLX;
            public short ThumbLY;
            public short ThumbRX;
            public short ThumbRY;
        }
        struct XInputState
        {
            public int dwPacketNumber;
            public XInputGamepad Gamepad;
        }
        const string XInputLibrary = "XINPUT9_1_0";
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);
        [SuppressUnmanagedCodeSecurity, DllImport(XInputLibrary)]
        static extern int XInputGetState(int dwUserIndex, out XInputState pState);
        static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }
        static object GetElementFromXInputState(XInputState state, EventType SourceEvent)
        {
            switch (SourceEvent)
            {
                case EventType.ThumbLX:
                    return state.Gamepad.ThumbLX;
                case EventType.ThumbLY:
                    return state.Gamepad.ThumbLY;
                case EventType.ThumbRX:
                    return state.Gamepad.ThumbRX;
                case EventType.ThumbRY:
                    return state.Gamepad.ThumbRY;
                case EventType.LeftTrigger:
                    return state.Gamepad.LeftTrigger;
                case EventType.RightTrigger:
                    return state.Gamepad.RightTrigger;
                case EventType.Buttons:
                    return state.Gamepad.Buttons;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
