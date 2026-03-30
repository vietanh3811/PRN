using System.Data.Odbc;

namespace FlowerStore.Data;

public sealed class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string chưa được cấu hình trong appsettings.json.");
        }

        _connectionString = connectionString;
    }

    public OdbcConnection CreateOpenConnection()
    {
        var connection = new OdbcConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
