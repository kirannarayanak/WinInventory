namespace WinInventory.Models;
 
public class MacBookSpec
{
    public string Model { get; set; } = "";
    public string Chip { get; set; } = "";
    public int CoresCpu { get; set; }
    public int CoresGpu { get; set; }
    public int RamGb { get; set; }
    public int StorageGb { get; set; }
    public double DisplayInches { get; set; }
    public int DisplayNits { get; set; }
    public int RefreshHz { get; set; }
    public double WeightKg { get; set; }
    public string Ports { get; set; } = "";
    public int MsrpAed { get; set; }
    public DateTime LaunchDate { get; set; }
    public double BatteryWh { get; set; }
    public string Wifi { get; set; } = "";
}