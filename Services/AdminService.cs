namespace WinInventory.Services;

public class AdminService
{
    private readonly HashSet<string> _adminEmails;
    
    public AdminService(IConfiguration configuration)
    {
        var adminEmails = configuration.GetSection("Admin:AdminEmails").Get<List<string>>() ?? new List<string>();
        _adminEmails = new HashSet<string>(adminEmails, StringComparer.OrdinalIgnoreCase);
    }
    
    public bool IsAdmin(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
            
        return _adminEmails.Contains(email);
    }
    
    public List<string> GetAdminEmails()
    {
        return _adminEmails.ToList();
    }
}

