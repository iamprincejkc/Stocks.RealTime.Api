
namespace Stocks.Hub
{
    public interface IStockUpdateClient
    {
        Task ReceiveStockPriceUpdate(StockPriceUpdate update);
    }
}