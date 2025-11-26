using WinInventory.Models;

namespace WinInventory.Services;

public class AppCompatibilityService
{
    private readonly Dictionary<string, CompatibilityType> _appCompatibilityMap = new()
    {
        // Native macOS apps
        { "microsoft office", CompatibilityType.NativeMacOS },
        { "adobe photoshop", CompatibilityType.NativeMacOS },
        { "adobe illustrator", CompatibilityType.NativeMacOS },
        { "adobe premiere", CompatibilityType.NativeMacOS },
        { "figma", CompatibilityType.NativeMacOS },
        { "slack", CompatibilityType.NativeMacOS },
        { "zoom", CompatibilityType.NativeMacOS },
        { "chrome", CompatibilityType.NativeMacOS },
        { "firefox", CompatibilityType.NativeMacOS },
        { "spotify", CompatibilityType.NativeMacOS },
        { "visual studio code", CompatibilityType.NativeMacOS },
        { "vscode", CompatibilityType.NativeMacOS },
        { "docker", CompatibilityType.NativeMacOS },
        { "cursor", CompatibilityType.NativeMacOS },
        { "postman", CompatibilityType.NativeMacOS },
        { "insomnia", CompatibilityType.NativeMacOS },
        { "sublime text", CompatibilityType.NativeMacOS },
        { "atom", CompatibilityType.NativeMacOS },
        { "node.js", CompatibilityType.NativeMacOS },
        { "nodejs", CompatibilityType.NativeMacOS },
        { "git", CompatibilityType.NativeMacOS },
        { "python", CompatibilityType.NativeMacOS },
        { "teams", CompatibilityType.NativeMacOS },
        
        // Web/SaaS apps
        { "microsoft teams", CompatibilityType.WebSaaS },
        { "outlook", CompatibilityType.WebSaaS },
        { "gmail", CompatibilityType.WebSaaS },
        { "google workspace", CompatibilityType.WebSaaS },
        { "salesforce", CompatibilityType.WebSaaS },
        { "notion", CompatibilityType.WebSaaS },
        
        // Rosetta 2 compatible
        { "autocad", CompatibilityType.Rosetta2Compatible },
        { "solidworks", CompatibilityType.Rosetta2Compatible },
        
        // Requires virtualization
        { "visual studio", CompatibilityType.RequiresVirtualization },
        { "sql server management", CompatibilityType.RequiresVirtualization },
        { "active directory", CompatibilityType.RequiresVirtualization },
        
        // Alternatives available
        { "microsoft project", CompatibilityType.AlternativeAvailable },
        { "visio", CompatibilityType.AlternativeAvailable },
    };

    public List<AppCompatibility> CheckCompatibility(List<string> appNames)
    {
        var results = new List<AppCompatibility>();
        
        foreach (var appName in appNames)
        {
            var appLower = appName.ToLowerInvariant();
            
            // Skip frameworks and development tools - only show actual applications
            if (IsFrameworkOrDevelopmentTool(appLower))
            {
                continue; // Skip this item entirely
            }
            
            var matched = false;
            
            // Try exact matches first, then partial matches
            // Check for VS Code first (most specific)
            if (appLower.Contains("visual studio code") || appLower.Contains("vscode"))
            {
                results.Add(new AppCompatibility
                {
                    AppName = appName,
                    Type = CompatibilityType.NativeMacOS,
                    CompatibilityScore = 1.0,
                    Notes = "Visual Studio Code available natively on macOS - excellent developer experience"
                });
                matched = true;
            }
            else
            {
                foreach (var (key, type) in _appCompatibilityMap)
                {
                    if (appLower.Contains(key))
                    {
                        results.Add(new AppCompatibility
                        {
                            AppName = appName,
                            Type = type,
                            CompatibilityScore = GetCompatibilityScore(type),
                            Notes = GetCompatibilityNotes(type, appName)
                        });
                        matched = true;
                        break;
                    }
                }
            }
            
            if (!matched)
            {
                // Better detection for Microsoft apps - check VS Code FIRST
                else if (appLower.Contains("microsoft") || appLower.Contains("ms "))
                {
                    // Check Visual Studio Code FIRST (before Office, before generic Visual Studio)
                    if (appLower.Contains("visual studio code") || appLower.Contains("vscode") || 
                        (appLower.Contains("code") && !appLower.Contains("sdk") && !appLower.Contains(".net") && !appLower.Contains("update")))
                    {
                        results.Add(new AppCompatibility
                        {
                            AppName = appName,
                            Type = CompatibilityType.NativeMacOS,
                            CompatibilityScore = 1.0,
                            Notes = "Visual Studio Code available natively on macOS - excellent developer experience"
                        });
                    }
                    else if (appLower.Contains("office") || appLower.Contains("365") || appLower.Contains("word") || 
                        appLower.Contains("excel") || appLower.Contains("powerpoint") || appLower.Contains("outlook"))
                    {
                        results.Add(new AppCompatibility
                        {
                            AppName = appName,
                            Type = CompatibilityType.NativeMacOS,
                            CompatibilityScore = 1.0,
                            Notes = "Microsoft Office available natively on macOS"
                        });
                    }
                    else if (appLower.Contains("visual studio") && !appLower.Contains("code") && !appLower.Contains("vscode"))
                    {
                        results.Add(new AppCompatibility
                        {
                            AppName = appName,
                            Type = CompatibilityType.RequiresVirtualization,
                            CompatibilityScore = 0.7,
                            Notes = "Visual Studio (not Code) requires Parallels Desktop or alternative solution"
                        });
                    }
                    else if (appLower.Contains("edge"))
                    {
                        results.Add(new AppCompatibility
                        {
                            AppName = appName,
                            Type = CompatibilityType.NativeMacOS,
                            CompatibilityScore = 1.0,
                            Notes = "Microsoft Edge available natively on macOS"
                        });
                    }
                    else if (appLower.Contains("onedrive"))
                    {
                        results.Add(new AppCompatibility
                        {
                            AppName = appName,
                            Type = CompatibilityType.NativeMacOS,
                            CompatibilityScore = 1.0,
                            Notes = "OneDrive available natively on macOS"
                        });
                    }
                    else
                    {
                        results.Add(new AppCompatibility
                        {
                            AppName = appName,
                            Type = CompatibilityType.RequiresVirtualization,
                            CompatibilityScore = 0.7,
                            Notes = "May require Parallels or alternative solution"
                        });
                    }
                }
                else if (appLower.Contains("chrome") || appLower.Contains("google chrome"))
                {
                    results.Add(new AppCompatibility
                    {
                        AppName = appName,
                        Type = CompatibilityType.NativeMacOS,
                        CompatibilityScore = 1.0,
                        Notes = "Fully native macOS app - optimal performance"
                    });
                }
                else if (appLower.Contains("node") || appLower.Contains("git") || appLower.Contains("python") || 
                         appLower.Contains("cursor") || appLower.Contains("docker") || appLower.Contains("postman") ||
                         appLower.Contains("insomnia") || appLower.Contains("sublime") || appLower.Contains("atom"))
                {
                    results.Add(new AppCompatibility
                    {
                        AppName = appName,
                        Type = CompatibilityType.NativeMacOS,
                        CompatibilityScore = 1.0,
                        Notes = "Available natively on macOS - excellent developer tools"
                    });
                }
                else if (appLower.Contains("zoom") || appLower.Contains("teams") || appLower.Contains("slack"))
                {
                    results.Add(new AppCompatibility
                    {
                        AppName = appName,
                        Type = CompatibilityType.NativeMacOS,
                        CompatibilityScore = 1.0,
                        Notes = "Fully native macOS app - optimal performance"
                    });
                }
                else if (appLower.Contains("windows") && !appLower.Contains("update") && !appLower.Contains("sdk"))
                {
                    results.Add(new AppCompatibility
                    {
                        AppName = appName,
                        Type = CompatibilityType.RequiresVirtualization,
                        CompatibilityScore = 0.7,
                        Notes = "Windows-specific - may require alternative or virtualization"
                    });
                }
                else if (appLower.Contains("update health") || appLower.Contains("health tools"))
                {
                    results.Add(new AppCompatibility
                    {
                        AppName = appName,
                        Type = CompatibilityType.RequiresVirtualization,
                        CompatibilityScore = 0.7,
                        Notes = "Windows-specific tool - may require alternative or virtualization"
                    });
                }
                else
                {
                    // Default: assume web-based or has alternative
                    results.Add(new AppCompatibility
                    {
                        AppName = appName,
                        Type = CompatibilityType.WebSaaS,
                        CompatibilityScore = 0.9,
                        Notes = "Likely available as web app or macOS alternative"
                    });
                }
            }
        }
        
        return results;
    }

    private double GetCompatibilityScore(CompatibilityType type)
    {
        return type switch
        {
            CompatibilityType.NativeMacOS => 1.0,
            CompatibilityType.WebSaaS => 1.0,
            CompatibilityType.Rosetta2Compatible => 0.95,
            CompatibilityType.AlternativeAvailable => 0.85,
            CompatibilityType.RequiresVirtualization => 0.75,
            CompatibilityType.NotCompatible => 0.3,
            _ => 0.8
        };
    }

    private string GetCompatibilityNotes(CompatibilityType type, string appName)
    {
        return type switch
        {
            CompatibilityType.NativeMacOS => "Fully native macOS app - optimal performance",
            CompatibilityType.WebSaaS => "Available as web app - works perfectly in browser",
            CompatibilityType.Rosetta2Compatible => "Runs via Rosetta 2 - excellent compatibility",
            CompatibilityType.AlternativeAvailable => $"Native macOS alternative available (e.g., {GetAlternative(appName)})",
            CompatibilityType.RequiresVirtualization => "Requires Parallels Desktop or similar - good performance",
            CompatibilityType.NotCompatible => "Limited compatibility - may need alternative solution",
            _ => "Compatibility varies"
        };
    }

    private string GetAlternative(string appName)
    {
        var lower = appName.ToLowerInvariant();
        if (lower.Contains("project")) return "OmniPlan or Asana";
        if (lower.Contains("visio")) return "Lucidchart or OmniGraffle";
        return "macOS alternative";
    }

    public double GetOverallCompatibilityScore(List<AppCompatibility> compatibilities)
    {
        if (compatibilities.Count == 0) return 1.0;
        return compatibilities.Average(c => c.CompatibilityScore);
    }

    private bool IsFrameworkOrDevelopmentTool(string appLower)
    {
        // Skip .NET Framework components
        if (appLower.Contains(".net framework") || 
            appLower.Contains("targeting pack") || 
            appLower.Contains("multi-targeting") || 
            appLower.Contains("bootstrapper") ||
            (appLower.Contains("microsoft .net") && !appLower.Contains("office")) ||
            (appLower.Contains("sdk") && appLower.Contains(".net") && !appLower.Contains("core") && 
             !appLower.Contains("5") && !appLower.Contains("6") && !appLower.Contains("7") && !appLower.Contains("8")))
        {
            return true;
        }
        
        // Skip other development frameworks and tools
        if (appLower.Contains("clickonce") ||
            appLower.Contains("kudu") ||
            appLower.Contains("iisnode") ||
            appLower.Contains("url rewrite") ||
            appLower.Contains("mercurial") && appLower.Contains("x86") ||
            (appLower.Contains("active directory") && appLower.Contains("library")))
        {
            return true;
        }
        
        // Skip Windows Update and system components
        if (appLower.Contains("update health") ||
            appLower.Contains("health tools") ||
            appLower.Contains("system component") ||
            (appLower.Contains("microsoft") && appLower.Contains("framework") && !appLower.Contains("office")))
        {
            return true;
        }
        
        return false;
    }
}

