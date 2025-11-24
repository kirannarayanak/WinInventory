namespace WinInventory.Models;

public class UserMachineData
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty; // "Google" or "Microsoft"
    public DateTime SignInTime { get; set; }
    public MachineInfo MachineInfo { get; set; } = new();
    public List<string> InstalledApplications { get; set; } = new();
}

