using System.Text.RegularExpressions;
using WinInventory.Models;
 
namespace WinInventory.Services;
 
public class MatcherService
{
    // Efficiency multipliers: Macs are more efficient, so lower specs can match higher Windows specs
    private const double MAC_CPU_EFFICIENCY_MULTIPLIER = 1.35; // Mac CPU ~35% more efficient per core
    private const double MAC_RAM_EFFICIENCY_MULTIPLIER = 1.25; // Mac unified memory ~25% more efficient
    private const double MAC_STORAGE_EFFICIENCY_MULTIPLIER = 1.10; // Mac storage slightly more efficient

    private static double CpuScoreWindows(string cpuName, int physicalCores)
    {
        cpuName = cpuName?.ToLowerInvariant() ?? "";
        double baseTier =
            cpuName.Contains("i9") || cpuName.Contains("ryzen 9") ? 0.90 :
            cpuName.Contains("i7") || cpuName.Contains("ryzen 7") ? 0.80 :
            cpuName.Contains("i5") || cpuName.Contains("ryzen 5") ? 0.65 :
            cpuName.Contains("i3") || cpuName.Contains("ryzen 3") ? 0.50 : 0.60;

        var m = Regex.Match(cpuName, @"\b(1[234]\d{3})\b");
        if (m.Success) baseTier += 0.05;

        if (physicalCores >= 12) baseTier += 0.08;
        else if (physicalCores >= 8) baseTier += 0.05;
        else if (physicalCores >= 6) baseTier += 0.02;

        return Math.Clamp(baseTier, 0, 1);
    }

    private static double CpuScoreMac(string chip, int cpuCores)
    {
        chip = chip?.ToLowerInvariant() ?? "";
        double baseTier =
            chip.Contains("max") ? 0.95 :
            chip.Contains("ultra") ? 0.98 :
            chip.Contains("pro") ? 0.88 :
            chip.Contains("m3")  ? 0.85 :
            chip.Contains("m2")  ? 0.75 :
            chip.Contains("m1")  ? 0.65 : 0.70;

        if (cpuCores >= 12) baseTier += 0.03;
        else if (cpuCores >= 10) baseTier += 0.02;
        else if (cpuCores >= 8) baseTier += 0.01;

        // Apply efficiency multiplier - Mac CPUs are more efficient
        return Math.Clamp(baseTier * MAC_CPU_EFFICIENCY_MULTIPLIER, 0, 1);
    }

    private static double NormRam(int gb)     => Math.Clamp((gb - 8) / 56.0, 0, 1);
    private static double NormStorage(int gb) => Math.Clamp((gb - 256) / 1792.0, 0, 1);

    // Adjusted normalization for Mac RAM - accounts for efficiency
    private static double NormRamMac(int gb) => NormRam((int)(gb * MAC_RAM_EFFICIENCY_MULTIPLIER));
    
    // Adjusted normalization for Mac Storage - accounts for efficiency
    private static double NormStorageMac(int gb) => NormStorage((int)(gb * MAC_STORAGE_EFFICIENCY_MULTIPLIER));

    private static double Cosine((double c,double r,double s) a, (double c,double r,double s) b)
    {
        double dot = a.c*b.c + a.r*b.r + a.s*b.s;
        double na  = Math.Sqrt(a.c*a.c + a.r*a.r + a.s*a.s);
        double nb  = Math.Sqrt(b.c*b.c + b.r*b.r + b.s*b.s);
        if (na == 0 || nb == 0) return 0;
        return dot / (na * nb);
    }

    // Enhanced similarity calculation that accounts for Mac efficiency
    private static double CalculateEfficiencyAdjustedSimilarity(
        (double c, double r, double s) winVec,
        (double c, double r, double s) macVec)
    {
        // Base cosine similarity
        double baseSim = Cosine(winVec, macVec);
        
        // Bonus for Mac efficiency: if Mac has lower raw specs but similar effective performance
        double cpuEfficiencyBonus = 0;
        if (macVec.c >= winVec.c * 0.85) // Mac CPU is at least 85% of Windows score
        {
            // Mac is efficient enough to match, give bonus
            cpuEfficiencyBonus = 0.05;
        }
        
        double ramEfficiencyBonus = 0;
        if (macVec.r >= winVec.r * 0.80) // Mac RAM is at least 80% of Windows score
        {
            ramEfficiencyBonus = 0.03;
        }
        
        double storageEfficiencyBonus = 0;
        if (macVec.s >= winVec.s * 0.90) // Mac storage is at least 90% of Windows score
        {
            storageEfficiencyBonus = 0.02;
        }
        
        // Total similarity with efficiency bonuses
        double totalSim = baseSim + cpuEfficiencyBonus + ramEfficiencyBonus + storageEfficiencyBonus;
        return Math.Clamp(totalSim, 0, 1);
    }

    public List<MatchResult> Recommend(MachineInfo win, IEnumerable<MacBookSpec> macs, PersonaWeights? personaWeights = null)
    {
        // Apply persona weights if provided
        var cpuWeight = personaWeights?.CpuWeight ?? 1.0;
        var ramWeight = personaWeights?.RamWeight ?? 1.0;
        var storageWeight = personaWeights?.StorageWeight ?? 1.0;
        var gpuWeight = personaWeights?.GpuWeight ?? 1.0;
        var batteryWeight = personaWeights?.BatteryWeight ?? 1.0;
        var portabilityWeight = personaWeights?.PortabilityWeight ?? 1.0;

        var winVec = (
            c: CpuScoreWindows(win.Processor, win.PhysicalCores) * cpuWeight,
            r: NormRam(ParseGb(win.TotalMemoryGB)) * ramWeight,
            s: NormStorage(GuessWindowsStorageGb(win)) * storageWeight
        );

        var results = new List<MatchResult>();
        foreach (var mac in macs)
        {
            // Use efficiency-adjusted normalization for Mac, with persona weights
            var macVec = (
                c: CpuScoreMac(mac.Chip, mac.CoresCpu) * cpuWeight,
                r: NormRamMac(mac.RamGb) * ramWeight,
                s: NormStorageMac(mac.StorageGb) * storageWeight
            );

            // Use enhanced similarity calculation
            var sim = CalculateEfficiencyAdjustedSimilarity(winVec, macVec);
            
            // Add persona-specific bonuses
            double personaBonus = 0;
            if (personaWeights != null)
            {
                // GPU bonus for designers
                if (gpuWeight > 1.0 && mac.CoresGpu >= 10) personaBonus += 0.02;
                // Battery bonus for office workers/students
                if (batteryWeight > 1.0 && mac.BatteryWh >= 50) personaBonus += 0.02;
                // Portability bonus (lighter = better)
                if (portabilityWeight > 1.0 && mac.WeightKg <= 1.5) personaBonus += 0.02;
            }
            sim = Math.Clamp(sim + personaBonus, 0, 1);

            // Calculate effective equivalent specs for display
            int effectiveRam = (int)(mac.RamGb * MAC_RAM_EFFICIENCY_MULTIPLIER);
            int effectiveStorage = (int)(mac.StorageGb * MAC_STORAGE_EFFICIENCY_MULTIPLIER);
            
            int winRam = ParseGb(win.TotalMemoryGB);
            int winStorage = GuessWindowsStorageGb(win);

            string cpuNote = EfficiencyNote(macVec.c, winVec.c, mac.Chip, win.Processor, "CPU");
            string ramNote = EfficiencyCapNote(mac.RamGb, effectiveRam, winRam, "RAM");
            string stoNote = EfficiencyCapNote(mac.StorageGb, effectiveStorage, winStorage, "Storage");
            string price   = mac.MsrpAed > 0 ? $"AED {mac.MsrpAed:n0}" : "—";

            results.Add(new MatchResult
            {
                Mac = mac,
                Similarity = Math.Round(sim, 3),
                CpuNote = cpuNote,
                RamNote = ramNote,
                StorageNote = stoNote,
                PriceNote = price
            });
        }

        // Smart sorting: Prefer cost-effective models (especially Air) when similarity is sufficient
        // Also ensure we get different Mac models for Good/Better/Best
        var sorted = results
            .OrderByDescending(r => 
            {
                // Calculate a composite score that balances similarity and cost-effectiveness
                double score = r.Similarity;
                
                // Bonus for Air models when similarity is good (>= 0.85)
                // This ensures Air models are preferred when they're sufficient
                bool isAir = r.Mac.Model.ToLowerInvariant().Contains("air");
                if (isAir && r.Similarity >= 0.85)
                {
                    score += 0.05; // Small bonus for Air when sufficient
                }
                
                // Cost-effectiveness bonus: prefer cheaper models when similarity is close
                // If similarity is >= 0.90, heavily weight cost-effectiveness
                if (r.Similarity >= 0.90 && r.Mac.MsrpAed > 0)
                {
                    // Normalize price (lower is better) - max price in catalog is ~10000
                    double priceScore = 1.0 - (r.Mac.MsrpAed / 10000.0);
                    score += priceScore * 0.10; // Up to 10% bonus for lower price
                }
                
                return score;
            })
            .ThenBy(r => r.Mac.MsrpAed == 0 ? int.MaxValue : r.Mac.MsrpAed) // Secondary: prefer cheaper
            .ToList();
        
        // Ensure we have different models for Good/Better/Best
        var unique = new List<MatchResult>();
        var seenModels = new HashSet<string>();
        
        foreach (var r in sorted)
        {
            var modelKey = $"{r.Mac.Model}_{r.Mac.RamGb}_{r.Mac.StorageGb}";
            if (!seenModels.Contains(modelKey))
            {
                unique.Add(r);
                seenModels.Add(modelKey);
                if (unique.Count >= 3) break;
            }
        }
        
        // If we don't have 3 unique, add remaining even if similar
        if (unique.Count < 3)
        {
            foreach (var r in sorted)
            {
                if (unique.Count >= 3) break;
                if (!unique.Contains(r))
                {
                    unique.Add(r);
                }
            }
        }
        
        return unique.Take(3).ToList();
    }
 
    private static int ParseGb(string memText)
    {
        if (string.IsNullOrWhiteSpace(memText)) return 8;
        var parts = memText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return double.TryParse(parts[0], out var gb) ? (int)Math.Round(gb) : 8;
    }
 
    private static int GuessWindowsStorageGb(MachineInfo win)
    {
        var sizes = win.Disks.Select(d => TryParseGb(d.SizeGB)).Where(v => v > 0).ToList();
        return sizes.Count == 0 ? 256 : sizes.Max();
    }
 
    private static int TryParseGb(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return double.TryParse(parts[0], out var gb) ? (int)Math.Round(gb) : 0;
    }
 
    private static string EfficiencyCapNote(int macRaw, int macEffective, int win, string label)
    {
        // Compare effective Mac specs (accounting for efficiency) to Windows
        if (macEffective >= win * 0.95 && macEffective <= win * 1.05)
        {
            return $"{label}: equivalent (Mac {macRaw} GB ≈ Win {win} GB)";
        }
        if (macEffective > win)
        {
            return $"{label}: ↑ Mac better (Mac {macRaw} GB ≈ {macEffective} GB effective vs Win {win} GB)";
        }
        // Mac has lower raw specs but might still be sufficient
        if (macEffective >= win * 0.85)
        {
            return $"{label}: ~ Mac sufficient (Mac {macRaw} GB ≈ {macEffective} GB effective vs Win {win} GB)";
        }
        return $"{label}: ↓ Mac lower (Mac {macRaw} GB ≈ {macEffective} GB effective vs Win {win} GB)";
    }

    private static string EfficiencyNote(double macScore, double winScore, string macChip, string winCpu, string label)
    {
        var delta = macScore - winScore;
        if (Math.Abs(delta) < 0.08)
        {
            return $"{label}: equivalent (Mac efficient architecture)";
        }
        if (macScore > winScore)
        {
            return $"{label}: ↑ Mac stronger (efficient {macChip})";
        }
        // Even if Mac score is lower, it might be sufficient due to efficiency
        if (macScore >= winScore * 0.80)
        {
            return $"{label}: ~ Mac sufficient (efficient {macChip})";
        }
        return $"{label}: ↓ Mac weaker";
    }
}