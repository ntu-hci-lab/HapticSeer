using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFCaptureSample.API_Hook.FunctionInfo.XInputBaseHooker;

namespace WPFCaptureSample.API_Hook.FunctionInfo
{
    public class ControllerOutputFunctionSet : FunctionHookBase
    {
        private static List<Base_XInputSetState> AllControllerDllSet;
        private Base_XInputSetState XInputSetStateType;
        public ControllerOutputFunctionSet(Process process)
        {
            if (AllControllerDllSet == null)
                AllControllerDllSet = Base_XInputSetState.FetchAllChild();
            foreach (Base_XInputSetState _base in AllControllerDllSet)
            {
                foreach (ProcessModule ModuleName in process.Modules)
                {
                    string DllName = ModuleName.ModuleName;
                    if (DllName.ToLower().Trim().Equals(_base.DllName().ToLower().Trim()))
                    {
                        XInputSetStateType = (Base_XInputSetState)Activator.CreateInstance(_base.GetType());
                        return;
                    }
                }
            }
            throw new Exception("Cannot find any supported controller module!");
        }

        public Base_XInputSetState AccessXInputSetState()
        {
            return XInputSetStateType;
        }

        public override string DllName()
        {
            return XInputSetStateType.DllName();
        }
        public override string FuncName()
        {
            return XInputSetStateType.FuncName();
        }
        public override int ShellCodeSize()
        {
            return XInputSetStateType.ShellCodeSize();
        }
        public override byte[] ShellCode(IntPtr RemoteFunctionEntryPoint, IntPtr RemoteShellCodeBase, IntPtr SelfFunctionEntryPoint, out int ShellCodeEntryPointOffset)
        {
            return XInputSetStateType.ShellCode(RemoteFunctionEntryPoint, RemoteShellCodeBase, SelfFunctionEntryPoint, out ShellCodeEntryPointOffset);
        }
    }
}
