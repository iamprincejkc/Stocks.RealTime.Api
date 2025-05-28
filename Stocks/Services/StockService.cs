using Dapper;
using Stocks.Hub;
using Stocks.Models;
using Stocks.Stocks;

namespace Stocks.Services
{
    internal sealed class StockService(ActiveTickerManager activeTickerManager,ISqlDatasource datasource, StocksClient stocksClient, ILogger<StockService> logger)
    {
        public async Task<StockPriceResponse?> GetLatestStockPrice(string ticker)
        {
            try
            {
                // Attempt to get the stock price from the datasource
                var dbPrice = await GetLatestPriceFromDatabase(ticker);
                if (dbPrice != null)
                {
                    activeTickerManager.AddTicker(ticker);
                    return dbPrice;
                }
                // If not found, fetch from external API
               var apiPrice = await stocksClient.GetDataForTicker(ticker);
                if (apiPrice == null)
                {
                    return null;
                }

                await SavePriceToDatabase(apiPrice);


                activeTickerManager.AddTicker(ticker);

                return apiPrice;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching stock price for {Ticker}", ticker);
            }
            return null; // Return null if no data found or an error occurred
        }

        private async Task<StockPriceResponse?> GetLatestPriceFromDatabase(string ticker)
        {
            const string sql = """
                                SELECT ticker, price, [timestamp]
                                FROM stock_prices
                                WHERE ticker = @ticker
                                ORDER BY [timestamp] DESC
                                OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;
                            """;

            using var connection = datasource.CreateConnection();
            connection.Open();

            var result = await connection.QueryFirstOrDefaultAsync<StockPriceResponse>(
                sql, new { ticker });

            return result;
        }

        private async Task SavePriceToDatabase(StockPriceResponse response)
        {
            const string sql = """
        INSERT INTO stock_prices (ticker, price, timestamp)
        VALUES (@Ticker, @Price, @Timestamp)
    """;

            using var connection = datasource.CreateConnection();
            connection.Open();

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                response.Ticker,
                response.Price,
                Timestamp = response.Timestamp < new DateTime(1753, 1, 1) ? DateTime.UtcNow : response.Timestamp
            });

            logger.LogInformation("Inserted {Rows} row(s) for {Ticker} at {Timestamp}", rowsAffected, response.Ticker, response.Timestamp);
        }
    }
}
