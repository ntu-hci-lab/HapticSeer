using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBoxInputWrapper.API_Hook.FunctionInfo
{
    public class FunctionHookBase
    {
        public virtual string DllName()
        {
            return null;
        }
        public virtual string FuncName()
        {
            return null;
        }
        public virtual int ShellCodeSize()
        {
            return 0;
        }
        public virtual byte[] ShellCode(IntPtr RemoteFunctionEntryPoint, IntPtr RemoteShellCodeBase, IntPtr SelfFunctionEntryPoint, out int ShellCodeEntryPointOffset)
        {
            ShellCodeEntryPointOffset = 0;
            return null;
        }
    }
}
