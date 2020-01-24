using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCaptureSample.API_Hook.FunctionInfo.XInputBaseHooker;

namespace WPFCaptureSample.API_Hook.FunctionInfo
{
    public class ControllerInputFunctionSet : FunctionHookBase
    {
        private static List<Base_XInputGetState> AllControllerDllSet;
        private Base_XInputGetState XInputGetStateType;
        public ControllerInputFunctionSet(Process process)
        {
            if (AllControllerDllSet == null)
                AllControllerDllSet = Base_XInputGetState.FetchAllChild();

            foreach (Base_XInputGetState _base in AllControllerDllSet)
            {
                foreach (ProcessModule ModuleName in process.Modules)
                {
                    string DllName = ModuleName.ModuleName;
                    if (DllName.ToLower().Trim().Equals(_base.DllName().ToLower().Trim()))
                    {
                        XInputGetStateType = (Base_XInputGetState)Activator.CreateInstance(_base.GetType());
                        return;
                    }
                }
            }
            throw new Exception("Cannot find any supported controller module!");
        }

        public Base_XInputGetState AccessXInputGetState()
        {
            return XInputGetStateType;
        }

        public override string DllName()
        {
            return XInputGetStateType.DllName();
        }
        public override string FuncName()
        {
            return XInputGetStateType.FuncName();
        }
        public override int ShellCodeSize()
        {
            return XInputGetStateType.ShellCodeSize();
        }
        public override byte[] ShellCode(IntPtr RemoteFunctionEntryPoint, IntPtr RemoteShellCodeBase, IntPtr SelfFunctionEntryPoint, out int ShellCodeEntryPointOffset)
        {
            return XInputGetStateType.ShellCode(RemoteFunctionEntryPoint, RemoteShellCodeBase, SelfFunctionEntryPoint, out ShellCodeEntryPointOffset);
        }
    }
}
