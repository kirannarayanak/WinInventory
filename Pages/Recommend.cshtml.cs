using Microsoft.AspNetCore.Mvc.RazorPages;
using WinInventory.Models;
using WinInventory.Services;
 
namespace WinInventory.Pages;
 
public class RecommendModel : PageModel
{
    private readonly InventoryService _inventory;
    private readonly CatalogService _catalog;
    private readonly MatcherService _matcher;
 
    public MachineInfo WinInfo { get; private set; } = new();
    public List<MatchResult> Matches { get; private set; } = new();
 
    public RecommendModel(InventoryService inv, CatalogService cat, MatcherService match)
    {
        _inventory = inv; _catalog = cat; _matcher = match;
    }
 
    public void OnGet()
    {
        WinInfo = _inventory.GetMachineInfo();
        var macs = _catalog.GetMacCatalog();
        Matches = _matcher.Recommend(WinInfo, macs);
    }
}