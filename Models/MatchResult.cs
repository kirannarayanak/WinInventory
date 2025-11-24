namespace WinInventory.Models;
 
public class MatchResult
{
    public MacBookSpec Mac { get; set; } = new();
    public double Similarity { get; set; } // 0..1
    // Simple deltas for UI
    public string CpuNote { get; set; } = "";
    public string RamNote { get; set; } = "";
    public string StorageNote { get; set; } = "";
    public string PriceNote { get; set; } = "";
}