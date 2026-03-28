namespace FlowerStore.Models;

public sealed class FlowerStatistic
{
    public int FlowerId { get; set; }
    public string FlowerName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public double TotalRevenue { get; set; }
}
