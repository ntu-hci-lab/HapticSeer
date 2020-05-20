using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFCaptureSample
{
    public class PseudoXInput
    {
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
        public bool IsStart = true;
        ~PseudoXInput()
        {
            IsStart = false;
        }
        public PseudoXInput()
        {
            LoadLibrary(XInputLibrary);
            ThreadPool.QueueUserWorkItem(new WaitCallback((s) =>
            {
                XInputState state = new XInputState();
                while (IsStart)
                {
                    try
                    {
                        XInputGetState(0, out state);
                    }
                    catch (Exception e)
                    {

                    }
                }
            }), this);
        }

    }
}
