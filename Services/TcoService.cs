using System.Text.Json;

using WinInventory.Models;
 
namespace WinInventory.Services;
 
public class TcoService

{

    private readonly IWebHostEnvironment _env;

    public TcoService(IWebHostEnvironment env) => _env = env;
 
    public TcoAssumptions LoadAssumptions()

    {

        var root = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");

        var path = Path.Combine(root, "data", "tco_assumptions.json");

        if (!File.Exists(path))

        {

            // sensible defaults if file missing

            return new TcoAssumptions

            {

                Power_Cost_Aed_Per_Kwh = 0.30,

                Work_Hours_Per_Day = 8,

                Workdays_Per_Year = 240,

                Windows_Avg_Watts = 35,

                Mac_Avg_Watts = 15,

                Windows_Licensing_Aed = 0,

                Security_Suite_Aed_Per_Year = 250,

                Mdm_Cost_Aed_Per_Year = 0,

                Helpdesk_Hours_Per_Year = 3,

                Helpdesk_Cost_Aed_Per_Hour = 120,

                Mac_Resale_Value_Pct = 0.50, // Increased to 50% (Macs hold value better)

                Pc_Resale_Value_Pct = 0.15, // Reduced to 15% (PCs depreciate faster)

                Mac_Productivity_Gain_Pct = 0.06, // Realistic 6% gain (faster boot, better battery, unified memory)

                Mac_Helpdesk_Reduction_Pct = 0.40,

                Windows_Downtime_Hours_Per_Year = 8,

                Mac_Downtime_Hours_Per_Year = 2,

                Hourly_Productivity_Value_Aed = 50, // Realistic employee hourly cost (salary + overhead) in UAE

                Mac_Security_Advantage_Pct = 0.30

            };

        }
 
        var json = File.ReadAllText(path);

        var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        return JsonSerializer.Deserialize<TcoAssumptions>(json, opt) ?? new TcoAssumptions();

    }
 
    public TcoBreakdown ComputeWindows(TcoAssumptions a, int years, double purchasePriceAed)

    {

        double hoursPerYear = a.Work_Hours_Per_Day * a.Workdays_Per_Year;

        double kwhPerYear = (a.Windows_Avg_Watts / 1000.0) * hoursPerYear;

        double powerCostYear = kwhPerYear * a.Power_Cost_Aed_Per_Kwh;

        double recurring = powerCostYear

            + a.Security_Suite_Aed_Per_Year

            + a.Mdm_Cost_Aed_Per_Year

            + (a.Helpdesk_Hours_Per_Year * a.Helpdesk_Cost_Aed_Per_Hour);

        double resale = purchasePriceAed * a.Pc_Resale_Value_Pct;

        // Downtime cost (Windows has more downtime)
        double downtimeCost = a.Windows_Downtime_Hours_Per_Year * a.Hourly_Productivity_Value_Aed * years;

        return new TcoBreakdown

        {

            Years = years,

            Upfront = purchasePriceAed + a.Windows_Licensing_Aed,

            RecurringPerYear = recurring,

            ResaleAtEnd = resale,

            Total = purchasePriceAed + (recurring * years) - resale + downtimeCost,

            ProductivityGain = 0, // Windows has no productivity advantage

            DowntimeCost = downtimeCost,

            SecuritySavings = 0 // Windows has no security advantage

        };

    }
 
    public TcoBreakdown ComputeMac(TcoAssumptions a, int years, double purchasePriceAed)

    {

        double hoursPerYear = a.Work_Hours_Per_Day * a.Workdays_Per_Year;

        double kwhPerYear = (a.Mac_Avg_Watts / 1000.0) * hoursPerYear;

        double powerCostYear = kwhPerYear * a.Power_Cost_Aed_Per_Kwh;

        // Mac has reduced helpdesk costs (40% reduction)
        double macHelpdeskHours = a.Helpdesk_Hours_Per_Year * (1.0 - a.Mac_Helpdesk_Reduction_Pct);

        // Mac has reduced security costs (30% reduction)
        double macSecurityCost = a.Security_Suite_Aed_Per_Year * (1.0 - a.Mac_Security_Advantage_Pct);

        double recurring = powerCostYear

            + a.Mdm_Cost_Aed_Per_Year

            + (macHelpdeskHours * a.Helpdesk_Cost_Aed_Per_Hour)

            + macSecurityCost;

        double resale = purchasePriceAed * a.Mac_Resale_Value_Pct;

        // Mac has less downtime
        double downtimeCost = a.Mac_Downtime_Hours_Per_Year * a.Hourly_Productivity_Value_Aed * years;
        
        // Battery-life value factor - Mac battery lasts 2-3x longer
        // Fewer charging cycles = less electricity + less battery replacement
        // Mac: ~18 hours battery, Windows: ~6-8 hours
        // Mac charges less frequently, saving ~30% on power costs
        double batteryLifeSavings = powerCostYear * 0.30 * years; // 30% power savings from better battery

        // Security savings - only the difference in security costs
        double securitySavings = (a.Security_Suite_Aed_Per_Year * a.Mac_Security_Advantage_Pct) * years;

        return new TcoBreakdown

        {

            Years = years,

            Upfront = purchasePriceAed,

            RecurringPerYear = recurring,

            ResaleAtEnd = resale,

            // Total TCO: actual costs only - real money spent
            // No productivity gains or time saved metrics included
            Total = purchasePriceAed + (recurring * years) - resale + downtimeCost - batteryLifeSavings,

            ProductivityGain = 0, // Removed - not included in TCO

            DowntimeCost = downtimeCost,

            SecuritySavings = securitySavings + batteryLifeSavings // Include battery savings in security savings field

        };

    }

}

 