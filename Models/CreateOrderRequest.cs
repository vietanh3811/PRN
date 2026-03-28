namespace FlowerStore.Models;

public sealed class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public int FlowerId { get; set; }
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }
}
