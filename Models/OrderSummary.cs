namespace FlowerStore.Models;

public sealed class OrderSummary
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public double TotalAmount { get; set; }
    public string DisplayName => $"Đơn #{Id} - {CustomerName} - {OrderDate:dd/MM/yyyy}";
}
