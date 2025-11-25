using Microsoft.AspNetCore.Mvc.RazorPages;
using WinInventory.Models;
using WinInventory.Services;
using System.Security.Claims;
 
namespace WinInventory.Pages;
 
public class IndexModel : PageModel
{
    private readonly InventoryService _inventory;
    private readonly UserDataService _userDataService;
    public MachineInfo Info { get; private set; } = new();
    public bool HasImportedData { get; private set; }

    public IndexModel(InventoryService inventory, UserDataService userDataService)
    {
        _inventory = inventory;
        _userDataService = userDataService;
    }

    public void OnGet()
    {
        // First, check if user has imported data
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var userData = _userDataService.GetUserMachineData(userId);
            
            if (userData != null && userData.MachineInfo != null)
            {
                Info = userData.MachineInfo;
                HasImportedData = !string.IsNullOrWhiteSpace(Info.ComputerName) && 
                                 !Info.ComputerName.Contains("N/A") && 
                                 !Info.ComputerName.Contains("Azure") &&
                                 !string.IsNullOrWhiteSpace(Info.Manufacturer) &&
                                 !string.IsNullOrWhiteSpace(Info.Model);
                return;
            }
        }
        
        // Fallback to WMI data (for local testing or if no imported data)
        Info = _inventory.GetMachineInfo();
        // Check if we have real data (not empty/N/A/Azure server)
        HasImportedData = !string.IsNullOrWhiteSpace(Info.ComputerName) && 
                         !Info.ComputerName.Contains("N/A") && 
                         !Info.ComputerName.Contains("Azure") &&
                         !string.IsNullOrWhiteSpace(Info.Manufacturer) &&
                         !string.IsNullOrWhiteSpace(Info.Model);
    }
}