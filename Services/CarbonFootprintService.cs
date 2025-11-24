using WinInventory.Models;

namespace WinInventory.Services;

public class CarbonFootprintService
{
    // CO2 emissions per kWh (UAE grid average)
    private const double CO2_PER_KWH_KG = 0.5; // kg CO2 per kWh
    
    // Manufacturing emissions (kg CO2)
    private const double MAC_MANUFACTURING_CO2 = 300; // Average MacBook
    private const double PC_MANUFACTURING_CO2 = 250; // Average Windows laptop
    
    // Trees needed to offset 1 kg CO2 (approximate)
    private const double TREES_PER_KG_CO2 = 0.02; // ~50 kg CO2 per tree per year

    public CarbonFootprint CalculateFootprint(TcoAssumptions assumptions, TcoBreakdown windows, TcoBreakdown mac, int years)
    {
        // Calculate operational emissions (power consumption)
        double windowsKwhPerYear = (assumptions.Windows_Avg_Watts / 1000.0) * 
                                   assumptions.Work_Hours_Per_Day * 
                                   assumptions.Workdays_Per_Year;
        double macKwhPerYear = (assumptions.Mac_Avg_Watts / 1000.0) * 
                               assumptions.Work_Hours_Per_Day * 
                               assumptions.Workdays_Per_Year;
        
        double windowsOperationalCo2 = windowsKwhPerYear * CO2_PER_KWH_KG * years;
        double macOperationalCo2 = macKwhPerYear * CO2_PER_KWH_KG * years;
        
        // Total emissions (manufacturing + operational)
        double windowsTotalCo2 = PC_MANUFACTURING_CO2 + windowsOperationalCo2;
        double macTotalCo2 = MAC_MANUFACTURING_CO2 + macOperationalCo2;
        
        double savingsCo2 = windowsTotalCo2 - macTotalCo2;
        double equivalentTrees = savingsCo2 * TREES_PER_KG_CO2;
        
        return new CarbonFootprint
        {
            WindowsCo2Kg = Math.Round(windowsTotalCo2, 2),
            MacCo2Kg = Math.Round(macTotalCo2, 2),
            SavingsCo2Kg = Math.Round(savingsCo2, 2),
            EquivalentTrees = Math.Round(equivalentTrees, 1),
            Description = savingsCo2 > 0 
                ? $"Switching to Mac reduces carbon footprint by {Math.Round(savingsCo2, 1)} kg CO2, equivalent to {Math.Round(equivalentTrees, 1)} trees planted"
                : "Similar carbon footprint"
        };
    }
}

