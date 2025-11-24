using WinInventory.Models;

namespace WinInventory.Services;

public class PersonaService
{
    public PersonaWeights GetPersonaWeights(UserPersona persona)
    {
        return persona switch
        {
            UserPersona.Developer => new PersonaWeights
            {
                CpuWeight = 1.2,
                RamWeight = 1.3,
                StorageWeight = 1.1,
                GpuWeight = 0.8,
                BatteryWeight = 1.0,
                PortabilityWeight = 0.9,
                Description = "Developers need strong CPU and RAM for compiling, running VMs, and IDEs"
            },
            UserPersona.Designer => new PersonaWeights
            {
                CpuWeight = 1.1,
                RamWeight = 1.2,
                StorageWeight = 1.2,
                GpuWeight = 1.3,
                BatteryWeight = 1.0,
                PortabilityWeight = 1.0,
                Description = "Designers need GPU power for graphics work and color-accurate displays"
            },
            UserPersona.OfficeWorker => new PersonaWeights
            {
                CpuWeight = 0.9,
                RamWeight = 1.0,
                StorageWeight = 0.9,
                GpuWeight = 0.7,
                BatteryWeight = 1.2,
                PortabilityWeight = 1.1,
                Description = "Office workers prioritize battery life and portability"
            },
            UserPersona.ITAdmin => new PersonaWeights
            {
                CpuWeight = 1.1,
                RamWeight = 1.2,
                StorageWeight = 1.0,
                GpuWeight = 0.8,
                BatteryWeight = 1.0,
                PortabilityWeight = 1.0,
                Description = "IT admins need reliable performance for multiple tools and VMs"
            },
            UserPersona.DataAnalyst => new PersonaWeights
            {
                CpuWeight = 1.3,
                RamWeight = 1.4,
                StorageWeight = 1.1,
                GpuWeight = 0.9,
                BatteryWeight = 0.9,
                PortabilityWeight = 0.8,
                Description = "Data analysts need maximum CPU and RAM for large datasets"
            },
            UserPersona.Student => new PersonaWeights
            {
                CpuWeight = 0.9,
                RamWeight = 1.0,
                StorageWeight = 0.9,
                GpuWeight = 0.8,
                BatteryWeight = 1.3,
                PortabilityWeight = 1.2,
                Description = "Students need long battery life and portability for campus use"
            },
            _ => new PersonaWeights
            {
                CpuWeight = 1.0,
                RamWeight = 1.0,
                StorageWeight = 1.0,
                GpuWeight = 1.0,
                BatteryWeight = 1.0,
                PortabilityWeight = 1.0,
                Description = "General use - balanced performance"
            }
        };
    }

    public UserPersona DetectPersonaFromApps(List<string> apps)
    {
        var appLower = apps.Select(a => a.ToLowerInvariant()).ToList();
        
        // Developer indicators
        if (appLower.Any(a => a.Contains("visual studio") || a.Contains("intellij") || 
                             a.Contains("docker") || a.Contains("kubernetes") || 
                             a.Contains("git") || a.Contains("node") || a.Contains("python")))
            return UserPersona.Developer;
        
        // Designer indicators
        if (appLower.Any(a => a.Contains("photoshop") || a.Contains("illustrator") || 
                             a.Contains("figma") || a.Contains("sketch") || 
                             a.Contains("premiere") || a.Contains("after effects")))
            return UserPersona.Designer;
        
        // Data Analyst indicators
        if (appLower.Any(a => a.Contains("tableau") || a.Contains("power bi") || 
                             a.Contains("r studio") || a.Contains("jupyter") || 
                             a.Contains("matlab") || a.Contains("spss")))
            return UserPersona.DataAnalyst;
        
        // IT Admin indicators
        if (appLower.Any(a => a.Contains("vmware") || a.Contains("virtualbox") || 
                             a.Contains("putty") || a.Contains("wireshark") || 
                             a.Contains("active directory") || a.Contains("sccm")))
            return UserPersona.ITAdmin;
        
        // Office Worker (default for common office apps)
        if (appLower.Any(a => a.Contains("office") || a.Contains("outlook") || 
                             a.Contains("teams") || a.Contains("slack") || 
                             a.Contains("chrome") || a.Contains("edge")))
            return UserPersona.OfficeWorker;
        
        return UserPersona.General;
    }
}

