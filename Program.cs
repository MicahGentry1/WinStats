using System.Runtime.InteropServices;
using System.Runtime.Versioning;

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
    // Uptime and cores are cross-platform
    double uptimeHours = Environment.TickCount64 / 1000.0 / 3600.0;
    int coreCount = Environment.ProcessorCount;

    long totalMb = 0;
    long usedMb = 0;
    int batteryPercent = -1; // -1 means "unknown"

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        // Windows-specific memory and battery
        var memStatus = new MEMORYSTATUSEX();
        memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        NativeMethods.GlobalMemoryStatusEx(ref memStatus);
        totalMb = (long)(memStatus.ullTotalPhys / 1024 / 1024);
        long availMb = (long)(memStatus.ullAvailPhys / 1024 / 1024);
        usedMb = totalMb - availMb;

        if (NativeMethods.GetSystemPowerStatus(out SYSTEM_POWER_STATUS powerStatus))
        {
            if (powerStatus.BatteryLifePercent != 255)
                batteryPercent = powerStatus.BatteryLifePercent;
        }
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        // Parse /proc/meminfo for total and available memory
        var memInfo = File.ReadAllLines("/proc/meminfo")
            .Select(line => line.Split(':', StringSplitOptions.TrimEntries))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1].Replace(" kB", ""));

        if (memInfo.TryGetValue("MemTotal", out string? totalStr) &&
            long.TryParse(totalStr, out long totalKb))
        {
            totalMb = totalKb / 1024;
        }
        if (memInfo.TryGetValue("MemAvailable", out string? availStr) &&
            long.TryParse(availStr, out long availKb))
        {
            long availMb = availKb / 1024;
            usedMb = totalMb - availMb;
        }
        // Battery on Linux can be read from /sys/class/power_supply/BAT0/capacity,
        // but we skip for brevity.
    }
    // Add macOS support here if needed (using sysctl)

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

        if (TotalMemMb > 0)
            Console.WriteLine($"Memory:  {UsedMemMb} / {TotalMemMb} MB");
        else
            Console.WriteLine("Memory:  N/A");

        Console.Write("Battery: ");
        if (BatteryPercent >= 0)
        {
            Console.ForegroundColor = BatteryPercent < 20 ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine($"{BatteryPercent}%");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("N/A");
        }
    }
}

// Windows interop structs – keep them as they are
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