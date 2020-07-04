using System;
using System.Diagnostics;
using System.Management;

public static class CacheOptimizer
{
    static bool IsRyzen_3950X = false;
    const int TargetCCX_ID = 3;
    /// <summary>
    /// Check is the computer using Ryzen 3950X
    /// </summary>
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
    /// <summary>
    /// Set the affinity of all threads in this process.
    /// All threads will work in the same CCX.
    /// It helps reduce the latency of Crossing-CCX-L3-Cache-Accessing.
    /// </summary>
    /// <param name="TaretCCX_ID">For 3950X, there are four CCX in 3950X. Set all threads in the same ccx helps performance improvement.</param>
    public static void ResetAllAffinity(int TaretCCX_ID = TargetCCX_ID)
    { 
        // If it is not 3950X
        if (!IsRyzen_3950X)
            return; //Do not optimize

        IntPtr Affinity = (IntPtr)(255 << (TargetCCX_ID * 8));  //4 Core, 8 Threads in one CCX. Set Affinity as 2^8 - 1
        
        ProcessThreadCollection threads;
        threads = Process.GetCurrentProcess().Threads;  //Get all threads in this process
        foreach (ProcessThread thread in threads)
            thread.ProcessorAffinity = Affinity;    //Set the affinity
    }
}