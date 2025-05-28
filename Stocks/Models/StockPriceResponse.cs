namespace Stocks.Models
{
    public class StockPriceResponse
    {
        public string Ticker { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
