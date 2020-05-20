using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCaptureSample.API_Hook.FunctionInfo.XInputBaseHooker
{
    class XInput1_3_XInputSetState : Base_XInputSetState
    {
        private static byte[] _ShellCode = new byte[] { 197, 45, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 82, 72, 139, 2, 72, 141, 21, 237, 255, 255, 255, 72, 137, 2, 90, 72, 49, 192, 72, 137, 84, 36, 16, 137, 76, 36, 8, 72, 131, 236, 88, 199, 68, 36, 52, 0, 0, 0, 0, 255, 37, 195, 255, 255, 255 };

        public override int GetPriority()
        {
            return 1;
        }
        public override string DllName()
        {
            return "XInput1_3.dll";
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
            long ReturnBackAddress = (long)RemoteFunctionEntryPoint + 0x15;
            for (long i = 0, temp = ReturnBackAddress; i < 8; ++i, temp >>= 8)
                ShellCode[i] = (byte)(temp & 0xFF);
            ShellCodeEntryPointOffset = 0x10;
            RemoteDataAddress = (long)RemoteShellCodeBase + 0x08;
            return ShellCode;
        }
    }
}
