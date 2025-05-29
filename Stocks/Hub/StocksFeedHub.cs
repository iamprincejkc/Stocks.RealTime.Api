using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Stocks.Hub;
internal sealed class StocksFeedHub : Hub<IStockUpdateClient>
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connectionTickers = new();
    private readonly ActiveTickerManager _tickerManager;

    public StocksFeedHub(ActiveTickerManager tickerManager)
    {
        _tickerManager = tickerManager;
    }

    public async Task JoinStockGroup(string ticker)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ticker);

        var tickers = _connectionTickers.GetOrAdd(Context.ConnectionId, _ => new ConcurrentDictionary<string, byte>());

        if (tickers.TryAdd(ticker, 0))
        {
            _tickerManager.AddTicker(ticker);
        }

    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionTickers.TryRemove(Context.ConnectionId, out var tickers))
        {
            foreach (var ticker in tickers.Keys)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticker);
                _tickerManager.RemoveTicker(ticker);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
