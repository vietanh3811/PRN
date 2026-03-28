using System.Data.Odbc;
using FlowerStore.Models;

namespace FlowerStore.Data;

public sealed class OrderRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public OrderRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public List<OrderSummary> GetAll()
    {
        const string sql = """
                           SELECT o.Id,
                                  c.Name,
                                  o.OrderDate,
                                  COALESCE(SUM(od.Quantity * od.Price), 0) AS TotalAmount
                           FROM Orders o
                           INNER JOIN Customers c ON c.Id = o.CustomerId
                           LEFT JOIN OrderDetails od ON od.OrderId = o.Id
                           GROUP BY o.Id, c.Name, o.OrderDate
                           ORDER BY o.Id DESC
                           """;

        using var connection = _connectionFactory.CreateOpenConnection();
        using var command = new OdbcCommand(sql, connection);
        using var reader = command.ExecuteReader();

        var orders = new List<OrderSummary>();
        while (reader.Read())
        {
            orders.Add(new OrderSummary
            {
                Id = reader.GetInt32(0),
                CustomerName = reader.GetString(1),
                OrderDate = reader.GetDateTime(2),
                TotalAmount = Convert.ToDouble(reader.GetValue(3))
            });
        }

        return orders;
    }

    public void Create(CreateOrderRequest request)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var unitPrice = GetFlowerPriceAndValidateStock(connection, transaction, request.FlowerId, request.Quantity);
            var orderId = InsertOrder(connection, transaction, request.CustomerId, request.OrderDate);
            InsertOrderDetail(connection, transaction, orderId, request.FlowerId, request.Quantity, unitPrice);
            UpdateFlowerStock(connection, transaction, request.FlowerId, request.Quantity);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void UpdateOrderDate(int orderId, DateTime orderDate)
    {
        const string sql = """
                           UPDATE Orders
                           SET OrderDate = ?
                           WHERE Id = ?
                           """;

        using var connection = _connectionFactory.CreateOpenConnection();
        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@OrderDate", orderDate);
        command.Parameters.AddWithValue("@Id", orderId);

        var rows = command.ExecuteNonQuery();
        if (rows == 0)
        {
            throw new InvalidOperationException("Không tìm thấy đơn hàng cần cập nhật.");
        }
    }

    public FlowerStatistic? GetTopSellingFlower()
    {
        const string sql = """
                           SELECT TOP 1
                                  f.Id,
                                  f.Name,
                                  SUM(od.Quantity) AS TotalSold,
                                  SUM(od.Quantity * od.Price) AS TotalRevenue
                           FROM OrderDetails od
                           INNER JOIN Flowers f ON f.Id = od.FlowerId
                           GROUP BY f.Id, f.Name
                           ORDER BY SUM(od.Quantity) DESC, SUM(od.Quantity * od.Price) DESC
                           """;

        return GetStatistic(sql);
    }

    public FlowerStatistic? GetHighestRevenueFlower()
    {
        const string sql = """
                           SELECT TOP 1
                                  f.Id,
                                  f.Name,
                                  SUM(od.Quantity) AS TotalSold,
                                  SUM(od.Quantity * od.Price) AS TotalRevenue
                           FROM OrderDetails od
                           INNER JOIN Flowers f ON f.Id = od.FlowerId
                           GROUP BY f.Id, f.Name
                           ORDER BY SUM(od.Quantity * od.Price) DESC, SUM(od.Quantity) DESC
                           """;

        return GetStatistic(sql);
    }

    private FlowerStatistic? GetStatistic(string sql)
    {
        using var connection = _connectionFactory.CreateOpenConnection();
        using var command = new OdbcCommand(sql, connection);
        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return new FlowerStatistic
        {
            FlowerId = reader.GetInt32(0),
            FlowerName = reader.GetString(1),
            TotalSold = Convert.ToInt32(reader.GetValue(2)),
            TotalRevenue = Convert.ToDouble(reader.GetValue(3))
        };
    }

    private static double GetFlowerPriceAndValidateStock(
        OdbcConnection connection,
        OdbcTransaction transaction,
        int flowerId,
        int quantity)
    {
        const string sql = """
                           SELECT Price, Quantity
                           FROM Flowers
                           WHERE Id = ?
                           """;

        using var command = new OdbcCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@FlowerId", flowerId);
        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            throw new InvalidOperationException("Không tìm thấy hoa được chọn.");
        }

        var price = Convert.ToDouble(reader.GetValue(0));
        var stock = Convert.ToInt32(reader.GetValue(1));
        if (stock < quantity)
        {
            throw new InvalidOperationException("Số lượng tồn không đủ để tạo đơn hàng.");
        }

        return price;
    }

    private static int InsertOrder(
        OdbcConnection connection,
        OdbcTransaction transaction,
        int customerId,
        DateTime orderDate)
    {
        const string insertSql = """
                                 INSERT INTO Orders (CustomerId, OrderDate)
                                 VALUES (?, ?)
                                 """;
        const string identitySql = "SELECT @@IDENTITY";

        using var insertCommand = new OdbcCommand(insertSql, connection, transaction);
        insertCommand.Parameters.AddWithValue("@CustomerId", customerId);
        insertCommand.Parameters.AddWithValue("@OrderDate", orderDate);
        insertCommand.ExecuteNonQuery();

        using var identityCommand = new OdbcCommand(identitySql, connection, transaction);
        return Convert.ToInt32(identityCommand.ExecuteScalar());
    }

    private static void InsertOrderDetail(
        OdbcConnection connection,
        OdbcTransaction transaction,
        int orderId,
        int flowerId,
        int quantity,
        double price)
    {
        const string sql = """
                           INSERT INTO OrderDetails (OrderId, FlowerId, Quantity, Price)
                           VALUES (?, ?, ?, ?)
                           """;

        using var command = new OdbcCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@OrderId", orderId);
        command.Parameters.AddWithValue("@FlowerId", flowerId);
        command.Parameters.AddWithValue("@Quantity", quantity);
        command.Parameters.AddWithValue("@Price", price);
        command.ExecuteNonQuery();
    }

    private static void UpdateFlowerStock(
        OdbcConnection connection,
        OdbcTransaction transaction,
        int flowerId,
        int quantity)
    {
        const string sql = """
                           UPDATE Flowers
                           SET Quantity = Quantity - ?
                           WHERE Id = ?
                           """;

        using var command = new OdbcCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Quantity", quantity);
        command.Parameters.AddWithValue("@FlowerId", flowerId);
        command.ExecuteNonQuery();
    }
}
