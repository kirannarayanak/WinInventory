using System.Globalization;
using System.Management;
using System.Text;
using ClosedXML.Excel;
using Microsoft.Win32;
using WinInventory.Models;
 
namespace WinInventory.Services;
 
public class InventoryService
{
    // -------- Machine Info (WMI) --------
    public MachineInfo GetMachineInfo()
    {
        var mi = new MachineInfo();
 
        using (var csSearcher = new ManagementObjectSearcher("SELECT Name, Manufacturer, Model, TotalPhysicalMemory FROM Win32_ComputerSystem"))
        {
            var cs = csSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (cs != null)
            {
                mi.ComputerName = cs["Name"]?.ToString() ?? "";
                mi.Manufacturer = cs["Manufacturer"]?.ToString() ?? "";
                mi.Model        = cs["Model"]?.ToString() ?? "";
                var totalMemBytes = ConvertToUInt64(cs["TotalPhysicalMemory"]);
                mi.TotalMemoryGB  = $"{BytesToGB(totalMemBytes):0.##} GB";
            }
        }
 
        using (var osSearcher = new ManagementObjectSearcher("SELECT Caption, Version, BuildNumber FROM Win32_OperatingSystem"))
        {
            var os = osSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (os != null)
            {
                mi.OSName      = os["Caption"]?.ToString() ?? "";
                mi.OSVersion   = os["Version"]?.ToString() ?? "";
                mi.BuildNumber = os["BuildNumber"]?.ToString() ?? "";
            }
        }
 
        using (var cpuSearcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
        {
            var cpu = cpuSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
            if (cpu != null)
            {
                mi.Processor     = cpu["Name"]?.ToString() ?? "";
                mi.PhysicalCores = ConvertToInt(cpu["NumberOfCores"]);
                mi.LogicalCores  = ConvertToInt(cpu["NumberOfLogicalProcessors"]);
            }
        }
 
        using (var diskSearcher = new ManagementObjectSearcher("SELECT Name, FileSystem, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3"))
        {
            foreach (var d in diskSearcher.Get().Cast<ManagementObject>())
            {
                var sizeBytes = ConvertToUInt64(d["Size"]);
                var freeBytes = ConvertToUInt64(d["FreeSpace"]);
 
                mi.Disks.Add(new DiskInfo
                {
                    Name       = d["Name"]?.ToString() ?? "",
                    FileSystem = d["FileSystem"]?.ToString() ?? "",
                    SizeGB     = $"{BytesToGB(sizeBytes):0.##} GB",
                    FreeGB     = $"{BytesToGB(freeBytes):0.##} GB"
                });
            }
        }
 
        return mi;
    }
 
    // -------- Installed Applications (Registry) --------
    public List<InstalledApp> GetInstalledApplications()
    {
        var results = new List<InstalledApp>();
        ReadUninstallHive(RegistryHive.LocalMachine, RegistryView.Registry64, "x64", "Machine", results);
        ReadUninstallHive(RegistryHive.LocalMachine, RegistryView.Registry32, "x86", "Machine", results);
        ReadUninstallHive(RegistryHive.CurrentUser,  RegistryView.Registry64, "x64", "User",   results);
        ReadUninstallHive(RegistryHive.CurrentUser,  RegistryView.Registry32, "x86", "User",   results);
 
        return results
            .Where(a => !string.IsNullOrWhiteSpace(a.Name))
            .GroupBy(a => $"{a.Name}|{a.Version}|{a.Scope}|{a.Architecture}")
            .Select(g => g.First())
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
 
    private void ReadUninstallHive(RegistryHive hive, RegistryView view, string arch, string scope, List<InstalledApp> sink)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var uninstall = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (uninstall == null) return;
 
            foreach (var subName in uninstall.GetSubKeyNames())
            {
                using var sub = uninstall.OpenSubKey(subName);
                if (sub == null) continue;
 
                var systemComponent = Convert.ToInt32((sub.GetValue("SystemComponent") ?? 0)) == 1;
                var releaseType     = Convert.ToString(sub.GetValue("ReleaseType")) ?? "";
                var parentKeyName   = Convert.ToString(sub.GetValue("ParentKeyName")) ?? "";
 
                var displayName     = Convert.ToString(sub.GetValue("DisplayName")) ?? "";
                var displayVersion  = Convert.ToString(sub.GetValue("DisplayVersion")) ?? "";
                var publisher       = Convert.ToString(sub.GetValue("Publisher")) ?? "";
                var installDate     = Convert.ToString(sub.GetValue("InstallDate")) ?? "";
                var uninstallStr    = Convert.ToString(sub.GetValue("UninstallString")) ?? "";
 
                if (string.IsNullOrWhiteSpace(displayName)) continue;
                if (systemComponent) continue;
                if (!string.IsNullOrEmpty(releaseType) && releaseType.Contains("Update", StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(parentKeyName)) continue;
 
                sink.Add(new InstalledApp
                {
                    Name = displayName,
                    Version = displayVersion,
                    Publisher = publisher,
                    InstallDate = installDate,
                    UninstallString = uninstallStr,
                    Architecture = arch,
                    Scope = scope
                });
            }
        }
        catch { /* ignore permission oddities */ }
    }
 
    // -------- CSV/Excel helpers (used later too) --------
    public byte[] BuildAppsCsv(IEnumerable<InstalledApp> apps)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Name,Version,Publisher,InstallDate,Architecture,Scope,UninstallString");
        foreach (var a in apps)
        {
            sb.AppendLine(string.Join(",", new[]
            {
                Csv(a.Name), Csv(a.Version), Csv(a.Publisher), Csv(a.InstallDate),
                Csv(a.Architecture), Csv(a.Scope), Csv(a.UninstallString)
            }));
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
 
    public byte[] BuildAppsExcel(IEnumerable<InstalledApp> apps)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Applications");
        var headers = new[] { "Name", "Version", "Publisher", "InstallDate", "Architecture", "Scope", "UninstallString" };
        for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
        int row = 2;
        foreach (var a in apps)
        {
            ws.Cell(row, 1).Value = a.Name;
            ws.Cell(row, 2).Value = a.Version;
            ws.Cell(row, 3).Value = a.Publisher;
            ws.Cell(row, 4).Value = a.InstallDate;
            ws.Cell(row, 5).Value = a.Architecture;
            ws.Cell(row, 6).Value = a.Scope;
            ws.Cell(row, 7).Value = a.UninstallString;
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
 
    private static string Csv(string? s)
    {
        s ??= "";
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }
 
    private static double BytesToGB(ulong bytes) => bytes == 0 ? 0 : bytes / 1024.0 / 1024.0 / 1024.0;
    private static ulong ConvertToUInt64(object? value)
    {
        if (value == null) return 0;
        if (value is ulong ul) return ul;
        if (ulong.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            return parsed;
        return 0;
    }
    private static int ConvertToInt(object? value)
    {
        if (value == null) return 0;
        if (value is int i) return i;
        if (int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            return parsed;
        return 0;
    }
}