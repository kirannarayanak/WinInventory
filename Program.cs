using WinInventory.Services;
using WinInventory.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure Data Protection to persist keys (fixes key ring error)
// For cloud deployments, use environment variable or file system
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new System.IO.DirectoryInfo("/tmp/dataprotection-keys"))
    .SetApplicationName("WinInventory");

builder.Services.AddRazorPages();

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/login";
})
.AddGoogle(options =>
{
    // Try multiple ways to get the configuration (environment variables first, then appsettings)
    var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
        ?? builder.Configuration["GOOGLE_CLIENT_ID"] 
        ?? builder.Configuration["Authentication:Google:ClientId"];
    var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
        ?? builder.Configuration["GOOGLE_CLIENT_SECRET"]
        ?? builder.Configuration["Authentication:Google:ClientSecret"];
    
    // Only configure if real credentials are provided
    if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && 
        !clientId.Contains("YOUR_GOOGLE_CLIENT") && !clientSecret.Contains("YOUR_GOOGLE_CLIENT") &&
        clientId != "DISABLED" && clientSecret != "DISABLED")
    {
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
    }
    else
    {
        // Disable Google auth if credentials not configured
        options.ClientId = "DISABLED";
        options.ClientSecret = "DISABLED";
    }
})
.AddMicrosoftAccount(options =>
{
    // Try multiple ways to get the configuration (environment variables first, then appsettings)
    var clientId = Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_ID")
        ?? builder.Configuration["MICROSOFT_CLIENT_ID"]
        ?? builder.Configuration["Authentication:Microsoft:ClientId"];
    var clientSecret = Environment.GetEnvironmentVariable("MICROSOFT_CLIENT_SECRET")
        ?? builder.Configuration["MICROSOFT_CLIENT_SECRET"]
        ?? builder.Configuration["Authentication:Microsoft:ClientSecret"];
    
    // Only configure if real credentials are provided
    if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && 
        !clientId.Contains("YOUR_MICROSOFT_CLIENT") && !clientSecret.Contains("YOUR_MICROSOFT_CLIENT") &&
        clientId != "DISABLED" && clientSecret != "DISABLED")
    {
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
    }
    else
    {
        // Disable Microsoft auth if credentials not configured
        options.ClientId = "DISABLED";
        options.ClientSecret = "DISABLED";
    }
});

builder.Services.AddAuthorization();

// Configure JSON to serialize enums as strings
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
 
builder.Services.AddSingleton<InventoryService>();

builder.Services.AddSingleton<CatalogService>();

builder.Services.AddSingleton<MatcherService>();

builder.Services.AddSingleton<TcoService>();

builder.Services.AddSingleton<PersonaService>();

builder.Services.AddSingleton<AppCompatibilityService>();

builder.Services.AddSingleton<PortCompatibilityService>();

builder.Services.AddSingleton<CarbonFootprintService>();

builder.Services.AddSingleton<EnhancedRecommendationService>();

builder.Services.AddSingleton<UserDataService>();

// Configure forwarded headers for HTTPS detection (needed for cloud deployments like Render)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
 
var app = builder.Build();

// Use forwarded headers middleware
app.UseForwardedHeaders();

// Check if OAuth is properly configured (only require auth if real credentials are set)
bool IsOAuthConfigured()
{
    var googleId = builder.Configuration["Authentication:Google:ClientId"] ?? builder.Configuration["GOOGLE_CLIENT_ID"] ?? "";
    var googleSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? builder.Configuration["GOOGLE_CLIENT_SECRET"] ?? "";
    var msId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? builder.Configuration["MICROSOFT_CLIENT_ID"] ?? "";
    var msSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? builder.Configuration["MICROSOFT_CLIENT_SECRET"] ?? "";
    
    return (!string.IsNullOrEmpty(googleId) && !googleId.Contains("YOUR_GOOGLE_CLIENT") && !string.IsNullOrEmpty(googleSecret) && !googleSecret.Contains("YOUR_GOOGLE_CLIENT")) ||
           (!string.IsNullOrEmpty(msId) && !msId.Contains("YOUR_MICROSOFT_CLIENT") && !string.IsNullOrEmpty(msSecret) && !msSecret.Contains("YOUR_MICROSOFT_CLIENT"));
}

var authRequired = IsOAuthConfigured();
 
if (!app.Environment.IsDevelopment())

{

    app.UseExceptionHandler("/Error");

    app.UseHsts();

}
 
app.UseHttpsRedirection();
 
app.UseStaticFiles();
 
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
 
app.MapRazorPages();
 
// APIs

app.MapGet("/api/machineinfo", (InventoryService svc) => Results.Ok(svc.GetMachineInfo()));

app.MapGet("/api/apps",        (InventoryService svc) => Results.Ok(svc.GetInstalledApplications()));

// Auth check endpoint
app.MapGet("/api/auth/check", (HttpContext ctx) => 
{
    var isAuthenticated = ctx.User.Identity?.IsAuthenticated ?? false;
    if (isAuthenticated)
    {
        var email = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? 
                    ctx.User.FindFirst("email")?.Value ?? 
                    ctx.User.FindFirst("preferred_username")?.Value ?? "";
        var name = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? 
                   ctx.User.FindFirst("name")?.Value ?? "";
        var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        
        return Results.Ok(new { 
            authenticated = true,
            email = email,
            name = name,
            userId = userId
        });
    }
    return Results.Ok(new { authenticated = false });
});

// Save user machine data endpoint (requires auth)
app.MapPost("/api/user/save-machine-data", async (HttpContext ctx, UserDataService userDataService, InventoryService inventoryService) =>
{
    if (!ctx.User.Identity?.IsAuthenticated ?? true)
    {
        return Results.Unauthorized();
    }

    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var email = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? 
                ctx.User.FindFirst("email")?.Value ?? 
                ctx.User.FindFirst("preferred_username")?.Value ?? "";
    var name = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? 
               ctx.User.FindFirst("name")?.Value ?? "";
    // Determine provider from authentication type or claims
    var provider = "Unknown";
    if (ctx.User.Identity.AuthenticationType?.Contains("Google", StringComparison.OrdinalIgnoreCase) == true)
        provider = "Google";
    else if (ctx.User.Identity.AuthenticationType?.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) == true)
        provider = "Microsoft";
    else
    {
        // Try to get from claims
        var idp = ctx.User.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider")?.Value ??
                  ctx.User.FindFirst(System.Security.Claims.ClaimTypes.AuthenticationMethod)?.Value;
        if (idp?.Contains("google", StringComparison.OrdinalIgnoreCase) == true)
            provider = "Google";
        else if (idp?.Contains("microsoft", StringComparison.OrdinalIgnoreCase) == true)
            provider = "Microsoft";
    }

    // Get machine info
    var machineInfo = inventoryService.GetMachineInfo();
    var applications = inventoryService.GetInstalledApplications().Select(a => a.Name).ToList();

    // Save user and machine data
    userDataService.SaveUserMachineData(userId, email, name, provider, machineInfo, applications);

    return Results.Ok(new { 
        message = "Machine data saved successfully",
        userId = userId,
        email = email
    });
}).RequireAuthorization();

// Get user machine data endpoint (requires auth)
app.MapGet("/api/user/machine-data", (HttpContext ctx, UserDataService userDataService) =>
{
    if (!ctx.User.Identity?.IsAuthenticated ?? true)
    {
        return Results.Unauthorized();
    }

    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var data = userDataService.GetUserMachineData(userId);

    if (data == null)
    {
        return Results.NotFound(new { message = "No machine data found for user" });
    }

    return Results.Ok(data);
}).RequireAuthorization();

// Admin endpoint to view all stored user data (for viewing who signed in and their machines)
app.MapGet("/api/admin/all-users", (UserDataService userDataService) =>
{
    var allData = userDataService.GetAllUserData();
    return Results.Ok(new 
    { 
        totalUsers = allData.Count,
        users = allData.Select(u => new
        {
            userId = u.UserId,
            email = u.UserEmail,
            name = u.UserName,
            provider = u.Provider,
            signInTime = u.SignInTime,
            machineInfo = new
            {
                computerName = u.MachineInfo.ComputerName,
                manufacturer = u.MachineInfo.Manufacturer,
                model = u.MachineInfo.Model,
                processor = u.MachineInfo.Processor,
                totalMemoryGB = u.MachineInfo.TotalMemoryGB,
                osName = u.MachineInfo.OSName,
                osVersion = u.MachineInfo.OSVersion
            },
            applicationCount = u.InstalledApplications.Count
        })
    });
});
 
app.MapGet("/export/apps.csv", (InventoryService svc) =>

{

    var data = svc.BuildAppsCsv(svc.GetInstalledApplications());

    return Results.File(data, "text/csv", "installed_apps.csv");

});

app.MapGet("/export/apps.xlsx", (InventoryService svc) =>

{

    var data = svc.BuildAppsExcel(svc.GetInstalledApplications());

    return Results.File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "installed_apps.xlsx");

});

// Export full machine data for Mac recommendation
// Export command endpoint - returns the PowerShell one-liner
app.MapGet("/api/export/command", () =>
{
    // Working PowerShell command - tested and verified
    var cmd = @"powershell -ExecutionPolicy Bypass -Command ""$d=[Environment]::GetFolderPath('Desktop');$t=Get-Date -Format 'yyyy-MM-dd_HH-mm-ss';$f=Join-Path $d \""machine-data_$t.csv\"";$cs=Get-WmiObject Win32_ComputerSystem;$os=Get-WmiObject Win32_OperatingSystem;$cpu=Get-WmiObject Win32_Processor|Select-Object -First 1;$m=[math]::Round($cs.TotalPhysicalMemory/1GB,2);$disks=(Get-WmiObject Win32_LogicalDisk -Filter 'DriveType=3'|ForEach-Object{$dd=$_.DeviceID;$fs=$_.FileSystem;$sz=[math]::Round($_.Size/1GB,2);$fr=[math]::Round($_.FreeSpace/1GB,2);\""$dd $fs ${sz}GB ${fr}GB\""}) -join ';';$apps=@();$keys=@('HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*','HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*','HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*');foreach($k in $keys){try{$items=Get-ItemProperty $k -ErrorAction SilentlyContinue;foreach($item in $items){if($item.DisplayName -and !$item.SystemComponent -and !$item.ReleaseType){$apps+=$item.DisplayName}}}catch{}};$apps=$apps|Sort-Object -Unique;$appsList=$apps -join ';';$csv='ComputerName,Manufacturer,Model,OSName,OSVersion,BuildNumber,Processor,PhysicalCores,LogicalCores,TotalMemoryGB,Disks,Applications' + \""`n\"";$csv+='\""'+$cs.Name+'\""'+','+'\""'+$cs.Manufacturer+'\""'+','+'\""'+$cs.Model+'\""'+','+'\""'+$os.Caption+'\""'+','+'\""'+$os.Version+'\""'+','+'\""'+$os.BuildNumber+'\""'+','+'\""'+$cpu.Name+'\""'+','+$cpu.NumberOfCores+','+$cpu.NumberOfLogicalProcessors+','+$m+',\""'+$disks+'\""'+','+'\""'+$appsList+'\""';$csv | Out-File $f -Encoding UTF8 -NoNewline;Write-Host 'Machine data exported successfully!' -ForegroundColor Green;Write-Host ('Saved to: ' + $f) -ForegroundColor Cyan""";
    return Results.Ok(new { command = cmd });
});

app.MapGet("/export/machine-data.csv", (InventoryService svc) =>
{
    var machine = svc.GetMachineInfo();
    var apps = svc.GetInstalledApplications();
    
    var csv = new System.Text.StringBuilder();
    csv.AppendLine("ComputerName,Manufacturer,Model,OSName,OSVersion,BuildNumber,Processor,PhysicalCores,LogicalCores,TotalMemoryGB,Disks,Applications");
    
    var disks = string.Join(";", machine.Disks.Select(d => $"{d.Name} {d.FileSystem} {d.SizeGB} {d.FreeGB}"));
    var appNames = string.Join(";", apps.Select(a => a.Name));
    
    string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
    
    csv.AppendLine($"{CsvEscape(machine.ComputerName)},{CsvEscape(machine.Manufacturer)},{CsvEscape(machine.Model)},{CsvEscape(machine.OSName)},{CsvEscape(machine.OSVersion)},{CsvEscape(machine.BuildNumber)},{CsvEscape(machine.Processor)},{machine.PhysicalCores},{machine.LogicalCores},{CsvEscape(machine.TotalMemoryGB)},{CsvEscape(disks)},{CsvEscape(appNames)}");
    
    return Results.File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "machine-data.csv");
});

// Import machine data and get Mac recommendation
app.MapPost("/api/import/recommend", async (HttpRequest req, HttpContext ctx, UserDataService userDataService, CatalogService cat, MatcherService match, TcoService tco, PersonaService persona, EnhancedRecommendationService enhanced) =>
{
    try
    {
        using var reader = new StreamReader(req.Body);
        var csvContent = await reader.ReadToEndAsync();
        
        if (string.IsNullOrWhiteSpace(csvContent))
            return Results.BadRequest(new { error = "No data provided" });
        
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
            return Results.BadRequest(new { error = "Invalid CSV format" });
        
        var headers = lines[0].Split(',');
        var data = lines[1].Split(',');
        
        if (headers.Length != data.Length)
            return Results.BadRequest(new { error = "CSV header/data mismatch" });
        
        var machine = new MachineInfo();
        var apps = new List<string>();
        
        for (int i = 0; i < headers.Length; i++)
        {
            var header = headers[i].Trim().ToLowerInvariant();
            var value = data[i].Trim();
            
            switch (header)
            {
                case "computername": machine.ComputerName = value; break;
                case "manufacturer": machine.Manufacturer = value; break;
                case "model": machine.Model = value; break;
                case "osname": machine.OSName = value; break;
                case "osversion": machine.OSVersion = value; break;
                case "buildnumber": machine.BuildNumber = value; break;
                case "processor": machine.Processor = value; break;
                case "physicalcores": if (int.TryParse(value, out var pc)) machine.PhysicalCores = pc; break;
                case "logicalcores": if (int.TryParse(value, out var lc)) machine.LogicalCores = lc; break;
                case "totalmemorygb": machine.TotalMemoryGB = value; break;
                case "disks":
                    var diskParts = value.Split(';');
                    foreach (var dp in diskParts)
                    {
                        var parts = dp.Split(' ');
                        if (parts.Length >= 4)
                        {
                            machine.Disks.Add(new DiskInfo
                            {
                                Name = parts[0],
                                FileSystem = parts[1],
                                SizeGB = parts[2],
                                FreeGB = parts[3]
                            });
                        }
                    }
                    break;
                case "applications":
                    apps = value.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToList();
                    break;
            }
        }
        
        // Save imported data to UserDataService if user is authenticated
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var email = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            var name = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? 
                      ctx.User.FindFirst("name")?.Value ?? 
                      email;
            // Determine provider from authentication type or claims
            var provider = "Unknown";
            var authType = ctx.User.Identity?.AuthenticationType ?? "";
            if (authType.Contains("Google", StringComparison.OrdinalIgnoreCase))
                provider = "Google";
            else if (authType.Contains("Microsoft", StringComparison.OrdinalIgnoreCase))
                provider = "Microsoft";
            else
            {
                var idp = ctx.User.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider")?.Value ??
                          ctx.User.FindFirst(System.Security.Claims.ClaimTypes.AuthenticationMethod)?.Value;
                if (idp?.Contains("google", StringComparison.OrdinalIgnoreCase) == true)
                    provider = "Google";
                else if (idp?.Contains("microsoft", StringComparison.OrdinalIgnoreCase) == true)
                    provider = "Microsoft";
            }
            
            userDataService.SaveUserMachineData(userId, email, name, provider, machine, apps);
        }
        
        // Get Mac recommendations
        var macs = cat.GetMacCatalog();
        if (macs.Count == 0)
            return Results.NotFound(new { error = "No Mac catalog found" });
        
        // Get parameters from query
        int years = 3;
        if (int.TryParse(req.Query["years"], out var y) && (y == 3 || y == 5)) years = y;
        
        double winPrice = 0;
        if (double.TryParse(req.Query["windowsPrice"], out var wp) && wp >= 0) winPrice = wp;
        
        UserPersona? userPersona = null;
        if (Enum.TryParse<UserPersona>(req.Query["persona"], true, out var p))
        {
            userPersona = p;
        }
        
        // Get recommendation
        var recommended = match.Recommend(machine, macs, userPersona.HasValue ? persona.GetPersonaWeights(userPersona.Value) : null);
        
        // Get enhanced recommendation
        var enhancedRec = enhanced.GetEnhancedRecommendation(machine, macs, userPersona, apps, years, winPrice);
        
        return Results.Ok(enhancedRec);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error processing import: {ex.Message}");
    }
});
 
// Top match + TCO (3y or 5y), optional windowsPrice

app.MapGet("/api/recommend/tco", (HttpRequest req, HttpContext ctx, InventoryService inv, UserDataService userDataService, CatalogService cat, MatcherService match, TcoService tco) =>
{
    // Require authentication if OAuth is configured
    if (authRequired && (!ctx.User.Identity?.IsAuthenticated ?? true))
    {
        return Results.Unauthorized();
    }

    // Try to get imported data first, fallback to WMI
    MachineInfo win;
    if (ctx.User.Identity?.IsAuthenticated == true)
    {
        var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userData = userDataService.GetUserMachineData(userId);
        if (userData != null && userData.MachineInfo != null)
        {
            win = userData.MachineInfo;
        }
        else
        {
            win = inv.GetMachineInfo();
        }
    }
    else
    {
        win = inv.GetMachineInfo();
    }

    var macs = cat.GetMacCatalog();

    // Get persona if provided
    UserPersona? userPersona = null;
    if (Enum.TryParse<UserPersona>(req.Query["persona"], true, out var p)) userPersona = p;
    
    PersonaWeights? weights = null;
    if (userPersona.HasValue)
    {
        var personaService = req.HttpContext.RequestServices.GetRequiredService<PersonaService>();
        weights = personaService.GetPersonaWeights(userPersona.Value);
    }

    var recs = match.Recommend(win, macs, weights);

    if (recs.Count == 0) return Results.NotFound(new { message = "No catalog entries found (macbooks.csv missing?)" });
 
    int years = 3;

    if (int.TryParse(req.Query["years"], out var y) && (y == 3 || y == 5)) years = y;
 
    double winPrice = 0;

    if (double.TryParse(req.Query["windowsPrice"], out var wp) && wp >= 0) winPrice = wp;
    
    // If no price provided, estimate based on machine specs (rough estimate)
    if (winPrice == 0)
    {
        // Estimate Windows laptop price based on specs
        var memoryGB = 0.0;
        if (double.TryParse(win.TotalMemoryGB.Replace(" GB", "").Trim(), out var mem)) memoryGB = mem;
        
        // Estimate based on processor and RAM
        if (win.Processor.Contains("i7", StringComparison.OrdinalIgnoreCase))
            winPrice = memoryGB >= 16 ? 6000 : 5000;
        else if (win.Processor.Contains("i5", StringComparison.OrdinalIgnoreCase))
            winPrice = memoryGB >= 16 ? 4500 : 4000;
        else if (win.Processor.Contains("i3", StringComparison.OrdinalIgnoreCase))
            winPrice = 3000;
        else if (win.Processor.Contains("Ryzen 7", StringComparison.OrdinalIgnoreCase) || win.Processor.Contains("AMD Ryzen 7", StringComparison.OrdinalIgnoreCase))
            winPrice = memoryGB >= 16 ? 5500 : 4500;
        else if (win.Processor.Contains("Ryzen 5", StringComparison.OrdinalIgnoreCase) || win.Processor.Contains("AMD Ryzen 5", StringComparison.OrdinalIgnoreCase))
            winPrice = memoryGB >= 16 ? 4000 : 3500;
        else
            winPrice = 5000; // Default estimate for unknown processors
    }

    var top = recs[0];

    var a = tco.LoadAssumptions();
 
    var w = tco.ComputeWindows(a, years, winPrice);

    var m = tco.ComputeMac(a, years, top.Mac.MsrpAed);

    // Calculate realistic savings - TCO difference only (real costs)
    var savings = w.Total - m.Total;
    
    // Cap savings percentage to be realistic (max 50% savings)
    var pct = w.Total > 0 ? Math.Min((savings / w.Total) * 100.0, 50.0) : 0;

    // Generate Mac advantages - real cost savings only, no productivity metrics
    var macAdvantages = new List<string>
    {
        $"Unified memory architecture - more efficient RAM usage than Windows",
        $"Industry-leading battery life - work unplugged longer, less charging time",
        $"{a.Mac_Helpdesk_Reduction_Pct * 100:F0}% fewer helpdesk tickets - macOS is more stable",
        $"Built-in security features - no need for expensive antivirus software",
        $"Better resale value - retains {a.Mac_Resale_Value_Pct * 100:F0}% value vs {a.Pc_Resale_Value_Pct * 100:F0}% for Windows PCs",
        $"Silent operation - no fan noise during normal use"
    };

    // Generate recommendations - realistic and actionable
    var recommendations = new List<string>();
    if (top.Similarity >= 0.90)
    {
        recommendations.Add($"Excellent match - This Mac will meet or exceed your current Windows performance");
    }
    else if (top.Similarity >= 0.85)
    {
        recommendations.Add($"Good match - Mac's efficiency means it will perform similarly to your Windows machine");
    }
    
    if (savings > 2000)
    {
        recommendations.Add($"Significant savings: Save AED {savings:F0} over {years} years with Mac");
    }
    else if (savings > 0)
    {
        recommendations.Add($"Cost-effective: Mac offers better value with AED {savings:F0} savings");
    }
    
    if (top.Mac.Model.ToLowerInvariant().Contains("air"))
    {
        recommendations.Add("MacBook Air provides excellent value - perfect for most professional workloads");
    }
    else if (top.Mac.Model.ToLowerInvariant().Contains("pro"))
    {
        recommendations.Add("MacBook Pro offers professional-grade performance for demanding tasks");
    }
    
        // Removed productivity/time saved recommendations - only real costs

    return Results.Ok(new WinInventory.Models.TcoCompareDto

    {

        SuggestedModel = top.Mac.Model,

        Chip = top.Mac.Chip,

        RamGb = top.Mac.RamGb,

        StorageGb = top.Mac.StorageGb,

        PriceAed = top.Mac.MsrpAed,

        Similarity = top.Similarity,

        Windows = w,

        Mac = m,

        SavingsAed = Math.Round(savings, 2),

        SavingsPct = Math.Round(pct, 2),

        MacAdvantages = macAdvantages,

        Recommendations = recommendations,
        
        // Removed time saved metrics - only real costs
        Years = years

    });

});

// Enhanced recommendation API with persona and workload support
app.MapGet("/api/recommend/enhanced", (
    HttpRequest req,
    HttpContext ctx,
    InventoryService inv,
    UserDataService userDataService,
    CatalogService cat,
    EnhancedRecommendationService enhanced,
    PersonaService persona) =>
{
    // Require authentication if OAuth is configured
    if (authRequired && (!ctx.User.Identity?.IsAuthenticated ?? true))
    {
        return Results.Unauthorized();
    }
    
    // Try to get imported data first, fallback to WMI
    MachineInfo win;
    List<string> applications = new();
    if (ctx.User.Identity?.IsAuthenticated == true)
    {
        var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userData = userDataService.GetUserMachineData(userId);
        if (userData != null && userData.MachineInfo != null)
        {
            win = userData.MachineInfo;
            applications = userData.InstalledApplications ?? new List<string>();
        }
        else
        {
            win = inv.GetMachineInfo();
            var installedApps = inv.GetInstalledApplications();
            applications = installedApps.Select(a => a.Name).Take(20).ToList();
        }
    }
    else
    {
        win = inv.GetMachineInfo();
        var installedApps = inv.GetInstalledApplications();
        applications = installedApps.Select(a => a.Name).Take(20).ToList();
    }
    
    var macs = cat.GetMacCatalog();
    
    if (macs.Count == 0) return Results.NotFound(new { message = "No catalog entries found" });
    
    // Get parameters
    int years = 3;
    if (int.TryParse(req.Query["years"], out var y) && (y == 3 || y == 5)) years = y;
    
    double winPrice = 0;
    if (double.TryParse(req.Query["windowsPrice"], out var wp) && wp >= 0) winPrice = wp;
    
    UserPersona? userPersona = null;
    if (Enum.TryParse<UserPersona>(req.Query["persona"], true, out var p)) userPersona = p;
    
    // Override applications if provided in query
    var appsParam = req.Query["apps"].ToString();
    if (!string.IsNullOrWhiteSpace(appsParam))
    {
        applications = appsParam.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
    
    try
    {
        var recommendation = enhanced.GetEnhancedRecommendation(win, macs, userPersona, applications, years, winPrice);
        return Results.Ok(recommendation);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Good/Better/Best recommendation tiers
app.MapGet("/api/recommend/tiers", (
    HttpRequest req,
    HttpContext ctx,
    InventoryService inv,
    UserDataService userDataService,
    CatalogService cat,
    MatcherService match,
    TcoService tco) =>
{
    // Require authentication if OAuth is configured
    if (authRequired && (!ctx.User.Identity?.IsAuthenticated ?? true))
    {
        return Results.Unauthorized();
    }
    
    // Try to get imported data first, fallback to WMI
    MachineInfo win;
    if (ctx.User.Identity?.IsAuthenticated == true)
    {
        var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userData = userDataService.GetUserMachineData(userId);
        if (userData != null && userData.MachineInfo != null)
        {
            win = userData.MachineInfo;
        }
        else
        {
            win = inv.GetMachineInfo();
        }
    }
    else
    {
        win = inv.GetMachineInfo();
    }
    
    var macs = cat.GetMacCatalog();
    
    // Get persona if provided
    UserPersona? userPersona = null;
    if (Enum.TryParse<UserPersona>(req.Query["persona"], true, out var p)) userPersona = p;
    
    PersonaWeights? weights = null;
    if (userPersona.HasValue)
    {
        var personaService = req.HttpContext.RequestServices.GetRequiredService<PersonaService>();
        weights = personaService.GetPersonaWeights(userPersona.Value);
    }
    
    var recs = match.Recommend(win, macs, weights);
    
    if (recs.Count == 0) return Results.NotFound(new { message = "No matches found" });
    
    int years = 3;
    if (int.TryParse(req.Query["years"], out var y) && (y == 3 || y == 5)) years = y;
    
    double winPrice = 0;
    if (double.TryParse(req.Query["windowsPrice"], out var wp) && wp >= 0) winPrice = wp;
    
    // If no price provided, estimate based on machine specs (same logic as main TCO endpoint)
    if (winPrice == 0)
    {
        var memoryGB = 0.0;
        if (double.TryParse(win.TotalMemoryGB.Replace(" GB", "").Trim(), out var mem)) memoryGB = mem;
        
        if (win.Processor.Contains("i7", StringComparison.OrdinalIgnoreCase))
            winPrice = memoryGB >= 16 ? 6000 : 5000;
        else if (win.Processor.Contains("i5", StringComparison.OrdinalIgnoreCase))
            winPrice = memoryGB >= 16 ? 4500 : 4000;
        else if (win.Processor.Contains("i3", StringComparison.OrdinalIgnoreCase))
            winPrice = 3000;
        else if (win.Processor.Contains("Ryzen 7", StringComparison.OrdinalIgnoreCase) || win.Processor.Contains("AMD Ryzen 7", StringComparison.OrdinalIgnoreCase))
            winPrice = memoryGB >= 16 ? 5500 : 4500;
        else if (win.Processor.Contains("Ryzen 5", StringComparison.OrdinalIgnoreCase) || win.Processor.Contains("AMD Ryzen 5", StringComparison.OrdinalIgnoreCase))
            winPrice = memoryGB >= 16 ? 4000 : 3500;
        else
            winPrice = 5000; // Default estimate
    }
    
    var assumptions = tco.LoadAssumptions();
    var windowsTco = tco.ComputeWindows(assumptions, years, winPrice);
    
    var tiers = new List<object>();
    for (int i = 0; i < Math.Min(recs.Count, 3); i++)
    {
        var rec = recs[i];
        var macTco = tco.ComputeMac(assumptions, years, rec.Mac.MsrpAed);
        
        // Calculate realistic savings (TCO difference, not including productivity gains in TCO)
        var savings = windowsTco.Total - macTco.Total;
        
        // Show similarity with more precision to differentiate tiers
        // Use 4 decimal places to show subtle differences
        var roundedSimilarity = Math.Round(rec.Similarity, 4);
        
        // Ensure savings is positive (Mac should be cheaper)
        var displaySavings = Math.Max(savings, 0); // Don't show negative savings
        var savingsPct = windowsTco.Total > 0 ? Math.Round((savings / windowsTco.Total) * 100, 1) : 0;
        
        tiers.Add(new
        {
            Tier = i == 0 ? "Good" : i == 1 ? "Better" : "Best",
            Mac = rec.Mac,
            Similarity = roundedSimilarity,
            TotalCost = Math.Round(macTco.Total, 2),
            Savings = Math.Round(displaySavings, 2),
            SavingsPct = savingsPct > 0 ? savingsPct : 0 // Only show positive percentage
        });
    }
    
    return Results.Ok(tiers);
});

app.Run();

 