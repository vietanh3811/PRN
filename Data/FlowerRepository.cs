using System.Data.Odbc;
using FlowerStore.Models;

namespace FlowerStore.Data;

public sealed class FlowerRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public FlowerRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public List<Flower> GetAll()
    {
        const string sql = """
                           SELECT Id, Name, Price, Quantity
                           FROM Flowers
                           ORDER BY Id
                           """;

        using var connection = _connectionFactory.CreateOpenConnection();
        using var command = new OdbcCommand(sql, connection);
        using var reader = command.ExecuteReader();

        var flowers = new List<Flower>();
        while (reader.Read())
        {
            flowers.Add(new Flower
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = Convert.ToDouble(reader.GetValue(2)),
                Quantity = reader.GetInt32(3)
            });
        }

        return flowers;
    }

    public void Add(Flower flower)
    {
        const string sql = """
                           INSERT INTO Flowers (Id, Name, Price, Quantity)
                           VALUES (?, ?, ?, ?)
                           """;

        using var connection = _connectionFactory.CreateOpenConnection();
        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", flower.Id);
        command.Parameters.AddWithValue("@Name", flower.Name);
        command.Parameters.AddWithValue("@Price", flower.Price);
        command.Parameters.AddWithValue("@Quantity", flower.Quantity);
        command.ExecuteNonQuery();
    }

    public void Delete(int flowerId)
    {
        using var connection = _connectionFactory.CreateOpenConnection();

        if (!Exists(connection, flowerId))
        {
            throw new InvalidOperationException("Không tìm thấy hoa cần xóa.");
        }

        if (HasOrderDetails(connection, flowerId))
        {
            throw new InvalidOperationException("Không thể xóa hoa đã tồn tại trong đơn hàng.");
        }

        const string sql = """
                           DELETE FROM Flowers
                           WHERE Id = ?
                           """;

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", flowerId);
        command.ExecuteNonQuery();
    }

    private static bool Exists(OdbcConnection connection, int flowerId)
    {
        const string sql = """
                           SELECT COUNT(1)
                           FROM Flowers
                           WHERE Id = ?
                           """;

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", flowerId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static bool HasOrderDetails(OdbcConnection connection, int flowerId)
    {
        const string sql = """
                           SELECT COUNT(1)
                           FROM OrderDetails
                           WHERE FlowerId = ?
                           """;

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@FlowerId", flowerId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }
}
