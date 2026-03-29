using FlowerStore.Data;
using FlowerStore.Models;

namespace FlowerStore.Services;

public sealed class FlowerStoreService
{
    private readonly FlowerRepository _flowerRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly OrderRepository _orderRepository;

    public FlowerStoreService()
    {
        var connectionFactory = new DbConnectionFactory();
        _flowerRepository = new FlowerRepository(connectionFactory);
        _customerRepository = new CustomerRepository(connectionFactory);
        _orderRepository = new OrderRepository(connectionFactory);
    }

    public List<Flower> GetFlowers() => _flowerRepository.GetAll();

    public List<Customer> GetCustomers() => _customerRepository.GetAll();

    public List<OrderSummary> GetOrders() => _orderRepository.GetAll();

    public void AddFlower(Flower flower)
    {
        ValidateFlower(flower);
        _flowerRepository.Add(flower);
    }

    public void AddCustomer(Customer customer)
    {
        ValidateCustomer(customer);
        _customerRepository.Add(customer);
    }

    public void CreateOrder(CreateOrderRequest request)
    {
        if (request.CustomerId <= 0)
        {
            throw new InvalidOperationException("Bạn cần chọn khách hàng.");
        }

        if (request.FlowerId <= 0)
        {
            throw new InvalidOperationException("Bạn cần chọn hoa.");
        }

        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("Số lượng hoa phải lớn hơn 0.");
        }

        _orderRepository.Create(request);
    }

    public void UpdateOrderDate(int orderId, DateTime orderDate)
    {
        _orderRepository.UpdateOrderDate(orderId, orderDate);
    }

    public FlowerStatistic? GetTopSellingFlower() => _orderRepository.GetTopSellingFlower();

    public FlowerStatistic? GetHighestRevenueFlower() => _orderRepository.GetHighestRevenueFlower();

    private static void ValidateFlower(Flower flower)
    {
        if (flower.Id <= 0)
        {
            throw new InvalidOperationException("Mã hoa phải lớn hơn 0.");
        }

        if (string.IsNullOrWhiteSpace(flower.Name))
        {
            throw new InvalidOperationException("Tên hoa không được để trống.");
        }

        if (flower.Price <= 0)
        {
            throw new InvalidOperationException("Giá hoa phải lớn hơn 0.");
        }

        if (flower.Quantity < 0)
        {
            throw new InvalidOperationException("Số lượng tồn không được âm.");
        }
    }

    private static void ValidateCustomer(Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.Name))
        {
            throw new InvalidOperationException("Tên khách hàng không được để trống.");
        }
    }
}
