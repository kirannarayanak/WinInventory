namespace WinInventory.Models;
 
public class TcoAssumptions

{

    public string Region { get; set; } = "UAE";

    public double Power_Cost_Aed_Per_Kwh { get; set; }

    public int Work_Hours_Per_Day { get; set; }

    public int Workdays_Per_Year { get; set; }

    public double Windows_Avg_Watts { get; set; }

    public double Mac_Avg_Watts { get; set; }

    public int Windows_Licensing_Aed { get; set; }

    public int Security_Suite_Aed_Per_Year { get; set; }

    public int Mdm_Cost_Aed_Per_Year { get; set; }

    public double Helpdesk_Hours_Per_Year { get; set; }

    public double Helpdesk_Cost_Aed_Per_Hour { get; set; }

    public double Mac_Resale_Value_Pct { get; set; }

    public double Pc_Resale_Value_Pct { get; set; }

    // Enhanced Mac advantages
    public double Mac_Productivity_Gain_Pct { get; set; } = 0.15; // 15% productivity gain
    public double Mac_Helpdesk_Reduction_Pct { get; set; } = 0.40; // 40% less helpdesk time
    public double Windows_Downtime_Hours_Per_Year { get; set; } = 8; // Windows downtime
    public double Mac_Downtime_Hours_Per_Year { get; set; } = 2; // Mac downtime
    public double Hourly_Productivity_Value_Aed { get; set; } = 50; // Realistic employee hourly cost (salary + overhead) in UAE
    public double Mac_Security_Advantage_Pct { get; set; } = 0.30; // 30% security cost reduction
    public double Mac_Minutes_Saved_Per_Day { get; set; } = 10.0; // Measurable time saved per day

}
 
public class TcoBreakdown

{

    public int Years { get; set; }

    public double Upfront { get; set; }

    public double RecurringPerYear { get; set; }

    public double ResaleAtEnd { get; set; }

    public double Total { get; set; }

    // Enhanced metrics
    public double ProductivityGain { get; set; } // Mac productivity advantage
    public double DowntimeCost { get; set; } // Cost of downtime/repairs
    public double SecuritySavings { get; set; } // Security-related savings

}
 
public class TcoCompareDto

{

    public string SuggestedModel { get; set; } = "";

    public string Chip { get; set; } = "";

    public int RamGb { get; set; }

    public int StorageGb { get; set; }

    public int PriceAed { get; set; }

    public double Similarity { get; set; }
 
    public TcoBreakdown Windows { get; set; } = new();

    public TcoBreakdown Mac { get; set; } = new();

    public double SavingsAed { get; set; }

    public double SavingsPct { get; set; }

    // Enhanced insights
    public List<string> MacAdvantages { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public double TotalMacValue { get; set; } // Total value including productivity gains
    
    // Time saved calculation details (for explanation)
    public double? MacMinutesSavedPerDay { get; set; }
    public double? WorkdaysPerYear { get; set; }
    public double? HourlyProductivityValueAed { get; set; }
    public int Years { get; set; }

}

 