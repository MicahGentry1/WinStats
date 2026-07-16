using System.Runtime.InteropServices;

bool runOnce = args.Contains("--once");

do
{
    Console.Clear();
    var info = CollectStats();
    info.Print();

    if (!runOnce)
    {
        Thread.Sleep(2000);
    }
} while (!runOnce);

SystemInfo CollectStats()
{
    // Uptime
    double uptimeHours = Environment.TickCount64 / 1000.0 / 3600.0;

    // Memory
    var memStatus = new MEMORYSTATUSEX();
    memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
    NativeMethods.GlobalMemoryStatusEx(ref memStatus);
    long totalMb = (long)(memStatus.ullTotalPhys / 1024 / 1024);
    long availMb = (long)(memStatus.ullAvailPhys / 1024 / 1024);
    long usedMb = totalMb - availMb;

    // CPU cores
    int coreCount = Environment.ProcessorCount;

    // Battery
    int batteryPercent = 0;
    if (NativeMethods.GetSystemPowerStatus(out SYSTEM_POWER_STATUS powerStatus))
    {
        // BatteryLifePercent is 0-100, or 255 if unknown (e.g. desktop with no battery)
        if (powerStatus.BatteryLifePercent != 255)
        {
            batteryPercent = powerStatus.BatteryLifePercent;
        }
    }

    return new SystemInfo
    {
        UptimeHours = uptimeHours,
        TotalMemMb = totalMb,
        UsedMemMb = usedMb,
        BatteryPercent = batteryPercent,
        CoreCount = coreCount
    };
}

class SystemInfo
{
    public double UptimeHours { get; set; }
    public long TotalMemMb { get; set; }
    public long UsedMemMb { get; set; }
    public int BatteryPercent { get; set; }
    public int CoreCount { get; set; }

    public void Print()
    {
        Console.WriteLine("=== WinStats ===");
        Console.WriteLine($"Uptime:  {UptimeHours:F1} hours");
        Console.WriteLine($"Cores:   {CoreCount}");
        Console.WriteLine($"Memory:  {UsedMemMb} / {TotalMemMb} MB");

        Console.Write("Battery: ");
        Console.ForegroundColor = BatteryPercent < 20 ? ConsoleColor.Red : ConsoleColor.Green;
        Console.WriteLine($"{BatteryPercent}%");
        Console.ResetColor();
    }
}

[StructLayout(LayoutKind.Sequential)]
struct MEMORYSTATUSEX
{
    public uint dwLength;
    public uint dwMemoryLoad;
    public ulong ullTotalPhys;
    public ulong ullAvailPhys;
    public ulong ullTotalPageFile;
    public ulong ullAvailPageFile;
    public ulong ullTotalVirtual;
    public ulong ullAvailVirtual;
    public ulong ullAvailExtendedVirtual;
}

[StructLayout(LayoutKind.Sequential)]
struct SYSTEM_POWER_STATUS
{
    public byte ACLineStatus;
    public byte BatteryFlag;
    public byte BatteryLifePercent;
    public byte SystemStatusFlag;
    public int BatteryLifeTime;
    public int BatteryFullLifeTime;
}

static class NativeMethods
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);
}
