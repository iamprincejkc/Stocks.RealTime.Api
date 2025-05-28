using Stocks.Models;

namespace Stocks.Stocks;

public class StocksClient
{
    private readonly HttpClient httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl;

    public StocksClient(HttpClient httpClient, IConfiguration config)
    {
        this.httpClient = httpClient;
        _apiKey = config["Stocks:ApiKey"]!;
        _apiUrl = config["Stocks:ApiUrl"]!;
    }

    public async Task<StockPriceResponse?> GetDataForTicker(string ticker)
    {
        var url = $"{_apiUrl}quote-short/{ticker}?apikey={_apiKey}";
        var raw = await httpClient.GetFromJsonAsync<List<StockApiResponse>>(url);

        var apiResult = raw?.FirstOrDefault();
        if (apiResult is null)
            return null;

        return new StockPriceResponse
        {
            Ticker = apiResult.Symbol,
            Price = apiResult.Price,
            Timestamp = DateTime.UtcNow
        };
    }

    private class StockApiResponse
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
