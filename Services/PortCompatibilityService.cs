using WinInventory.Models;

namespace WinInventory.Services;

public class PortCompatibilityService
{
    public PortCompatibility CheckCompatibility(MachineInfo windowsMachine, MacBookSpec mac)
    {
        var missingPorts = new List<string>();
        var availablePorts = new List<string>();
        var needsHub = false;
        
        // Common Windows ports that might be needed
        var commonWindowsPorts = new HashSet<string> { "HDMI", "USB-A", "USB-C", "Ethernet", "SD Card", "VGA", "DisplayPort" };
        
        // Parse Mac ports
        var macPorts = mac.Ports.ToUpperInvariant();
        if (macPorts.Contains("HDMI")) availablePorts.Add("HDMI");
        if (macPorts.Contains("USB-A") || macPorts.Contains("USB 3")) availablePorts.Add("USB-A");
        if (macPorts.Contains("USB-C") || macPorts.Contains("TB") || macPorts.Contains("THUNDERBOLT")) 
        {
            availablePorts.Add("USB-C");
            availablePorts.Add("Thunderbolt");
        }
        if (macPorts.Contains("ETHERNET") || macPorts.Contains("RJ-45")) availablePorts.Add("Ethernet");
        if (macPorts.Contains("SD")) availablePorts.Add("SD Card");
        
        // Check for missing common ports
        if (!macPorts.Contains("HDMI") && !macPorts.Contains("THUNDERBOLT")) missingPorts.Add("HDMI");
        if (!macPorts.Contains("USB-A") && !macPorts.Contains("USB 3")) missingPorts.Add("USB-A");
        if (!macPorts.Contains("ETHERNET") && !macPorts.Contains("RJ-45")) missingPorts.Add("Ethernet");
        
        needsHub = missingPorts.Count > 0;
        
        var hubRecommendation = needsHub 
            ? $"Recommended: USB-C Hub with {string.Join(", ", missingPorts)} - AED 150-300"
            : "No hub needed - all ports available";
        
        var compatibilityScore = 1.0 - (missingPorts.Count * 0.15);
        if (compatibilityScore < 0) compatibilityScore = 0;
        
        return new PortCompatibility
        {
            NeedsHub = needsHub,
            MissingPorts = missingPorts,
            AvailablePorts = availablePorts,
            HubRecommendation = hubRecommendation,
            CompatibilityScore = compatibilityScore
        };
    }
}

