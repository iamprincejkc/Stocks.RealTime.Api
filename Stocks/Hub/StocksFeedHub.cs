using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace Stocks.Hub;
internal sealed class StocksFeedHub : Hub<IStockUpdateClient>
{
    public async Task JoinStockGroup(string ticker)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
    }
}
