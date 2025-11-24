using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WinInventory.Pages;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        // Sign out from all authentication schemes
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        
        // Clear authentication cookies specifically
        Response.Cookies.Delete(".AspNetCore.Cookies");
        Response.Cookies.Delete(".AspNetCore.Cookies");
        
        // Redirect to home page with a query parameter to force refresh
        return RedirectToPage("/Index", new { logout = "true" });
    }
}

