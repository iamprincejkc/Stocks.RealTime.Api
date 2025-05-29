using System.Collections.Concurrent;

namespace Stocks.Hub
{
    internal sealed class ActiveTickerManager
    {
        private readonly ConcurrentDictionary<string, int> _activeTickers = new();

        public void AddTicker(string ticker)
        {
            _activeTickers.AddOrUpdate(ticker, 1, (_, count) => count + 1);
        }

        public void RemoveTicker(string ticker)
        {
            if (_activeTickers.TryGetValue(ticker, out var count))
            {
                if (count <= 1)
                {
                    _activeTickers.TryRemove(ticker, out _);
                }
                else
                {
                    _activeTickers[ticker] = count - 1;
                }
            }
        }

        public IEnumerable<string> GetAllTicker()
        {
            return _activeTickers.Keys;
        }
    }
}
