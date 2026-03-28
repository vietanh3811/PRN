namespace FlowerStore.Data;

public static class DatabaseConfig
{
    private const string DefaultConnectionString =
        "Driver={SQL Server};Server=localhost;Database=FlowerStoreDB;Trusted_Connection=Yes;";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("FLOWER_STORE_CONNECTION_STRING") ?? DefaultConnectionString;
}
