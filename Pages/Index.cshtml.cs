using Microsoft.AspNetCore.Mvc.RazorPages;
using WinInventory.Models;
using WinInventory.Services;
 
namespace WinInventory.Pages;
 
public class IndexModel : PageModel
{
    private readonly InventoryService _inventory;
    public MachineInfo Info { get; private set; } = new();
 
    public IndexModel(InventoryService inventory) => _inventory = inventory;
 
    public void OnGet()
    {
        Info = _inventory.GetMachineInfo();
    }
}