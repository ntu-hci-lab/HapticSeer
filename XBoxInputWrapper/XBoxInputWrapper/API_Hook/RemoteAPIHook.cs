using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XBoxInputWrapper.API_Hook.FunctionInfo;

namespace XBoxInputWrapper.API_Hook
{
    public class RemoteAPIHook
    {

        // privileges
        class DesiredAccess
        {
            public const int PROCESS_CREATE_THREAD = 0x0002;
            public const int PROCESS_QUERY_INFORMATION = 0x0400;
            public const int PROCESS_VM_OPERATION = 0x0008;
            public const int PROCESS_VM_WRITE = 0x0020;
            public const int PROCESS_VM_READ = 0x0010;
            public const int PROCESS_ALL_ACCESS = (0x1F0FFF);
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibraryA(string lpLibFileName);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        class AllocationType
        {
            public const uint MEM_COMMIT = 0x00001000;
            public const uint MEM_RESERVE = 0x00002000;
        }
        public enum Protection
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out UIntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();
        [DllImport("kernel32.dll")]
        public static extern void SetLastError(int ErrorCode); 

        Process process;
        IntPtr pHandle;
        public RemoteAPIHook(Process proc)
        {
            process = proc;
            pHandle = OpenProcess(DesiredAccess.PROCESS_ALL_ACCESS, false, proc.Id);
        }
        public IntPtr RemoteAllocateExecutableMemory(int Size)
        {
            IntPtr VirtualMem = VirtualAllocEx(pHandle, IntPtr.Zero, Size, AllocationType.MEM_RESERVE | AllocationType.MEM_COMMIT, (uint)Protection.PAGE_READWRITE);
            if (VirtualMem.Equals(IntPtr.Zero))
                return IntPtr.Zero;
            uint dummy;
            VirtualProtectEx(pHandle, VirtualMem, (UIntPtr)Size, (uint)Protection.PAGE_EXECUTE_READWRITE, out dummy);
            return VirtualMem;
        }
        public bool Hook(FunctionHookBase func)
        {
            ProcessModule targetModule = null;
            foreach (ProcessModule ModuleName in process.Modules)
            {
                string DllName = ModuleName.ModuleName;
                if (!DllName.ToLower().Equals(func.DllName().ToLower().Trim()))
                    continue;
                targetModule = ModuleName;
                break;
            }
            if (targetModule == null)
                return false;

            IntPtr SelfDllBaseAddress = LoadLibraryA(targetModule.FileName);
            if (SelfDllBaseAddress == null)
                return false;

            IntPtr SelfFunctionEntryPoint = GetProcAddress(SelfDllBaseAddress, func.FuncName());
            if (SelfFunctionEntryPoint == null)
                return false;

            long Offset = (long)SelfFunctionEntryPoint - (long)SelfDllBaseAddress;
            IntPtr RemoteFunctionEntryPoint = IntPtr.Add(targetModule.BaseAddress, (int)Offset);

            IntPtr RemoteShellCodeBase = RemoteAllocateExecutableMemory(func.ShellCodeSize());
            int ShellCodeEntryOffset;
            byte[] ShellCode = func.ShellCode(RemoteFunctionEntryPoint, RemoteShellCodeBase, SelfFunctionEntryPoint, out ShellCodeEntryOffset);
            UIntPtr dummy;
            WriteProcessMemory(pHandle, RemoteShellCodeBase, ShellCode, ShellCode.Length, out dummy);

            long RemoteShellCodeEntryPoint = (long)RemoteShellCodeBase + ShellCodeEntryOffset;


            byte[] API_Jump_ShellCode = new byte[12];
            API_Jump_ShellCode[0] = 0x48;   //mov rax, $PseudoEntryPoint
            API_Jump_ShellCode[1] = 0xB8;
            for (long i = 0, temp = RemoteShellCodeEntryPoint; i < 8; ++i, temp >>= 8)
                API_Jump_ShellCode[2 + i] = (byte)(temp & 0xFF);
            API_Jump_ShellCode[10] = 0xFF;  //jmp rax
            API_Jump_ShellCode[11] = 0xE0;

            WriteProcessMemory(pHandle, RemoteFunctionEntryPoint, API_Jump_ShellCode, API_Jump_ShellCode.Length, out dummy);
            return true;
        }
        public void RemoteAddressRead(IntPtr RemoteAddress, ref byte[] Output)
        {
            if (Output == null)
                return;
            int dummy = 0;
            ReadProcessMemory(pHandle, RemoteAddress, Output, Output.Length, ref dummy);
            return;
        }
        ~RemoteAPIHook()
        {
            CloseHandle(pHandle);
        }
    }
}
