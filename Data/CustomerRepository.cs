using System.Data.Odbc;
using FlowerStore.Models;

namespace FlowerStore.Data;

public sealed class CustomerRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public CustomerRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public List<Customer> GetAll()
    {
        const string sql = """
                           SELECT Id, Name
                           FROM Customers
                           ORDER BY Name
                           """;

        using var connection = _connectionFactory.CreateOpenConnection();
        using var command = new OdbcCommand(sql, connection);
        using var reader = command.ExecuteReader();

        var customers = new List<Customer>();
        while (reader.Read())
        {
            customers.Add(new Customer
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }

        return customers;
    }
}
