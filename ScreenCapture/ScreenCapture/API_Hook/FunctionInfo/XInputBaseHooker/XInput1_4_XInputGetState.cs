using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCaptureSample.API_Hook.FunctionInfo.XInputBaseHooker
{
    class XInput1_4_XInputGetState : Base_XInputGetState
    {
        private static byte[] _ShellCode = new byte[] { 253, 22, 198, 110, 251, 127, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 81, 86, 87, 72, 139, 53, 238, 255, 255, 255, 72, 141, 61, 126, 0, 0, 0, 72, 185, 30, 0, 0, 0, 0, 0, 0, 0, 252, 243, 164, 72, 139, 13, 195, 255, 255, 255, 199, 5, 185, 255, 255, 255, 0, 0, 0, 0, 199, 5, 191, 255, 255, 255, 0, 0, 0, 0, 72, 137, 13, 176, 255, 255, 255, 95, 94, 89, 255, 37, 167, 255, 255, 255, 72, 139, 4, 36, 129, 61, 161, 255, 255, 255, 0, 0, 0, 0, 117, 240, 72, 137, 29, 152, 255, 255, 255, 72, 141, 29, 129, 255, 255, 255, 72, 137, 3, 72, 139, 29, 135, 255, 255, 255, 72, 141, 5, 136, 255, 255, 255, 72, 137, 4, 36, 72, 137, 92, 36, 8, 86, 87, 65, 86, 72, 131, 236, 48, 255, 37, 81, 255, 255, 255 };

        public override int GetPriority()
        {
            return 2;
        }
        public override string DllName()
        {
            return "XInput1_4.dll";
        }
        public override string FuncName()
        {
            return "XInputGetState";
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
            ShellCodeEntryPointOffset = 0x69;
            RemoteDataAddress = (long)RemoteShellCodeBase + 0xAF;
            return ShellCode;
        }
    }
}
