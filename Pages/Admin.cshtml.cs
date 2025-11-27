using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WinInventory.Services;

namespace WinInventory.Pages;

[Authorize]
public class AdminModel : PageModel
{
    private readonly AdminService _adminService;
    
    public AdminModel(AdminService adminService)
    {
        _adminService = adminService;
    }
    
    public IActionResult OnGet()
    {
        // Check if user is admin
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? 
                    User.FindFirst("email")?.Value ?? 
                    User.FindFirst("preferred_username")?.Value ?? "";
        
        if (!_adminService.IsAdmin(email))
        {
            return Forbid(); // Return 403 Forbidden
        }
        
        return Page();
    }
}

