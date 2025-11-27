using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WinInventory.Pages;

[Authorize]
public class AdminModel : PageModel
{
    public void OnGet()
    {
    }
}

