using System;
using System.Diagnostics;
using System.Management;
using System.Threading;

public static class CacheOptimizer
{
    static bool IsRyzen_3950X = false;
    const int TargetCCX_ID = 3;
    public static void Init()
    {
        ManagementObjectSearcher mos =
          new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
        foreach (ManagementObject mo in mos.Get())
        {
            string CPUName = mo["Name"].ToString();
            if (!CPUName.Contains("Ryzen"))
                continue;
            if (!CPUName.Contains("3950X"))
                continue;
            IsRyzen_3950X = true;
            break;
        }
    }
    public static void ResetAllAffinity(int TaretCCX_ID = TargetCCX_ID)
    {
        if (!IsRyzen_3950X)
            return;
        ProcessThreadCollection threads;
        IntPtr Affinity = (IntPtr)(255 << (TargetCCX_ID * 8));
        threads = Process.GetCurrentProcess().Threads;
        foreach (ProcessThread thread in threads)
            thread.ProcessorAffinity = Affinity;
    }
}