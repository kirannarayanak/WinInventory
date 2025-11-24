namespace WinInventory.Models;

public enum UserPersona
{
    Developer,
    Designer,
    OfficeWorker,
    ITAdmin,
    DataAnalyst,
    Student,
    General
}

public class PersonaWeights
{
    public double CpuWeight { get; set; } = 1.0;
    public double RamWeight { get; set; } = 1.0;
    public double StorageWeight { get; set; } = 1.0;
    public double GpuWeight { get; set; } = 1.0;
    public double BatteryWeight { get; set; } = 1.0;
    public double PortabilityWeight { get; set; } = 1.0;
    public string Description { get; set; } = "";
}

public class WorkloadProfile
{
    public UserPersona Persona { get; set; } = UserPersona.General;
    public List<string> Applications { get; set; } = new();
    public string WorkflowDescription { get; set; } = "";
    public int HoursPerDay { get; set; } = 8;
    public bool RequiresGpu { get; set; } = false;
    public bool RequiresHighRam { get; set; } = false;
    public bool RequiresPortability { get; set; } = false;
}

public class AppCompatibility
{
    public string AppName { get; set; } = "";
    public CompatibilityType Type { get; set; }
    public string Notes { get; set; } = "";
    public double CompatibilityScore { get; set; } = 1.0; // 0-1
}

public enum CompatibilityType
{
    NativeMacOS,
    WebSaaS,
    Rosetta2Compatible,
    RequiresVirtualization,
    NotCompatible,
    AlternativeAvailable
}

public class RecommendationTier
{
    public string Tier { get; set; } = ""; // "Good", "Better", "Best"
    public MacBookSpec Mac { get; set; } = new();
    public double Similarity { get; set; }
    public double TotalCost { get; set; }
    public string Rationale { get; set; } = "";
    public List<string> Advantages { get; set; } = new();
}

public class CarbonFootprint
{
    public double WindowsCo2Kg { get; set; }
    public double MacCo2Kg { get; set; }
    public double SavingsCo2Kg { get; set; }
    public double EquivalentTrees { get; set; } // Trees needed to offset
    public string Description { get; set; } = "";
}

public class PortCompatibility
{
    public bool NeedsHub { get; set; }
    public List<string> MissingPorts { get; set; } = new();
    public List<string> AvailablePorts { get; set; } = new();
    public string HubRecommendation { get; set; } = "";
    public double CompatibilityScore { get; set; } = 1.0;
}

public class PerformanceRadarData
{
    public Dictionary<string, double> WindowsData { get; set; } = new();
    public Dictionary<string, double> MacData { get; set; } = new();
}

public class MacAdvantage
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string WindowsLimitation { get; set; } = "";
}

public class EnhancedRecommendation
{
    public MacBookSpec RecommendedMac { get; set; } = new();
    public MacBookSpec CostOptimizedMac { get; set; } = new();
    public MacBookSpec PerformanceOptimizedMac { get; set; } = new();
    public double Similarity { get; set; }
    public string AIExplanation { get; set; } = "";
    public List<AppCompatibility> AppCompatibilities { get; set; } = new();
    public PortCompatibility PortCompatibility { get; set; } = new();
    public CarbonFootprint CarbonFootprint { get; set; } = new();
    public PerformanceRadarData PerformanceRadar { get; set; } = new();
    public List<string> WorkflowMatches { get; set; } = new();
    public List<MacAdvantage> MacAdvantages { get; set; } = new();
}

