namespace WinInventory.Models;
 
public class InstalledApp
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string InstallDate { get; set; } = ""; // keep raw (e.g., 20250121) if present
    public string UninstallString { get; set; } = "";
    public string Architecture { get; set; } = ""; // x64/x86
    public string Scope { get; set; } = "";        // Machine/User
}