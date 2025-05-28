
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Stocks.Models;
using Stocks.Services;

namespace Stocks.Hub;

internal sealed class StocksFeedUpdater(
    ActiveTickerManager activeTickerManager,
    IServiceScopeFactory serviceScopeFactory,
    IHubContext<StocksFeedHub, IStockUpdateClient> hubContext,
    IOptions<StockUpdateOptions> options,
    ILogger<StocksFeedUpdater> logger 
    ) : BackgroundService
{

    private readonly Random _random = new Random();
    private readonly StockUpdateOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            await UpdateStockPrices();
            await Task.Delay(_options.UpdateInterval, stoppingToken);
        }    
    }

    private async Task UpdateStockPrices()
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        StockService stockService = scope.ServiceProvider.GetRequiredService<StockService>();   

        foreach(string ticker in activeTickerManager.GetAllTicker())
        {
            try
            {
                var price = await stockService.GetLatestStockPrice(ticker);
                if (price == null)
                {
                    continue;
                }
                decimal newPrice = CalculateNewPrice(price);

                var update = new StockPriceUpdate(ticker, newPrice);

                await hubContext.Clients.Group(ticker).ReceiveStockPriceUpdate(update);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating stock price for {Ticker}", ticker);
            }
        }
    }

    private decimal CalculateNewPrice(StockPriceResponse currentPrice)
    {
        double change = _options.MaxPercentageChange;
        decimal priceFactor= (decimal)(_random.NextDouble() * change * 2 - change);
        decimal priceChange = currentPrice.Price * priceFactor;
        decimal newPrice = Math.Max(0,currentPrice.Price + priceChange);
        newPrice = Math.Round(newPrice, 2);
        return newPrice;
    }
}
