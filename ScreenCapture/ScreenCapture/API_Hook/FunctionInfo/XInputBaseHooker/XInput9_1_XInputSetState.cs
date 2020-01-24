using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCaptureSample.API_Hook.FunctionInfo.XInputBaseHooker
{
    class XInput9_1_XInputSetState : Base_XInputSetState
    {
        private static byte[] _ShellCode = new byte[] { 125, 17, 227, 72, 248, 127, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 83, 72, 139, 26, 72, 137, 29, 237, 255, 255, 255, 91, 72, 137, 92, 36, 8, 87, 72, 131, 236, 32, 72, 139, 218, 255, 37, 209, 255, 255, 255 };

        public override int GetPriority()
        {
            return 3;
        }
        public override string DllName()
        {
            return "xinput9_1_0.dll";
        }
        public override string FuncName()
        {
            return "XInputSetState";
        }
        public override int ShellCodeSize()
        {
            return (_ShellCode.Length & (~4095)) + 4096;  //Make sure allocate 4k*n memory
        }
        public override byte[] ShellCode(IntPtr RemoteFunctionEntryPoint, IntPtr RemoteShellCodeBase, IntPtr SelfFunctionEntryPoint, out int ShellCodeEntryPointOffset)
        {
            byte[] ShellCode = _ShellCode;
            long ReturnBackAddress = (long)RemoteFunctionEntryPoint + 0x0D;
            for (long i = 0, temp = ReturnBackAddress; i < 8; ++i, temp >>= 8)
                ShellCode[i] = (byte)(temp & 0xFF);
            ShellCodeEntryPointOffset = 0x10;
            RemoteDataAddress = (long)RemoteShellCodeBase + 0x08;
            return ShellCode;
        }
    }
}
