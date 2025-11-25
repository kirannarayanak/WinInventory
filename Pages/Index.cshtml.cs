using Microsoft.AspNetCore.Mvc.RazorPages;
using WinInventory.Models;
using WinInventory.Services;
 
namespace WinInventory.Pages;
 
public class IndexModel : PageModel
{
    private readonly InventoryService _inventory;
    public MachineInfo Info { get; private set; } = new();
    public bool HasImportedData { get; private set; }

    public IndexModel(InventoryService inventory) => _inventory = inventory;

    public void OnGet()
    {
        Info = _inventory.GetMachineInfo();
        // Check if we have real data (not empty/N/A/Azure server)
        HasImportedData = !string.IsNullOrWhiteSpace(Info.ComputerName) && 
                         !Info.ComputerName.Contains("N/A") && 
                         !Info.ComputerName.Contains("Azure") &&
                         !string.IsNullOrWhiteSpace(Info.Manufacturer) &&
                         !string.IsNullOrWhiteSpace(Info.Model);
    }
}