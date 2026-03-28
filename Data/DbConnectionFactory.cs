using System.Data.Odbc;

namespace FlowerStore.Data;

public sealed class DbConnectionFactory
{
    public OdbcConnection CreateOpenConnection()
    {
        var connection = new OdbcConnection(DatabaseConfig.ConnectionString);
        connection.Open();
        return connection;
    }
}
