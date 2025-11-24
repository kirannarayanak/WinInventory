 namespace WinInventory.Models;
 
public class MachineInfo
{
    public string ComputerName { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public string OSName { get; set; } = "";
    public string OSVersion { get; set; } = "";
    public string BuildNumber { get; set; } = "";
    public string Processor { get; set; } = "";
    public int LogicalCores { get; set; }
    public int PhysicalCores { get; set; }
    public string TotalMemoryGB { get; set; } = "";
    public List<DiskInfo> Disks { get; set; } = new();
}
 
public class DiskInfo
{
    public string Name { get; set; } = "";
    public string FileSystem { get; set; } = "";
    public string SizeGB { get; set; } = "";
    public string FreeGB { get; set; } = "";
}