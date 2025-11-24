using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WinInventory.Pages;

public class LoginModel : PageModel
{
    public IActionResult OnGet()
    {
        // If already authenticated, redirect to home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }
        return Page();
    }

    public IActionResult OnPostGoogle()
    {
        var redirectUrl = Url.Page("/Index");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    public IActionResult OnPostMicrosoft()
    {
        var redirectUrl = Url.Page("/Index");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
    }
}

