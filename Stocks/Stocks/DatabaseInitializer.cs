using Dapper;
using Microsoft.Data.SqlClient;
using Stocks.Services;

namespace Stocks.Stocks;

public class DatabaseInitializer(
    ISqlDatasource datasource,
    ILogger<DatabaseInitializer> logger,
    IConfiguration configuration
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await EnsureDatabaseAndTableExistAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database initialization.");
            throw;
        }
    }

    private async Task EnsureDatabaseAndTableExistAsync()
    {
        var originalBuilder = new SqlConnectionStringBuilder(configuration["STOCKS_CONNECTION_STRING"]!);
        string dbName = originalBuilder.InitialCatalog;

        // Connect to master to check/create the DB
        var masterBuilder = new SqlConnectionStringBuilder(originalBuilder.ConnectionString)
        {
            InitialCatalog = "master"
        };

        using var masterConnection = new SqlConnection(masterBuilder.ConnectionString);
        await masterConnection.OpenAsync();

        var dbExists = await masterConnection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sys.databases WHERE name = @dbName", new { dbName });

        if (dbExists == 0)
        {
            await masterConnection.ExecuteAsync($"CREATE DATABASE [{dbName}]");
        }

        // Now connect to the target DB and create tables
        using var dbConnection = new SqlConnection(originalBuilder.ConnectionString);
        await dbConnection.OpenAsync();

        const string sql = """
        IF NOT EXISTS (
            SELECT * FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = 'stock_prices'
        )
        BEGIN
            CREATE TABLE stock_prices (
                id INT IDENTITY(1,1) PRIMARY KEY,
                ticker VARCHAR(10) NOT NULL,
                price DECIMAL(18,2) NOT NULL,
                [timestamp] DATETIME2 NOT NULL DEFAULT GETDATE()
            );

            CREATE INDEX idx_stock_prices_ticker ON stock_prices(ticker);
            CREATE INDEX idx_stock_prices_timestamp ON stock_prices([timestamp]);
        END
    """;

        await dbConnection.ExecuteAsync(sql);
    }
}
