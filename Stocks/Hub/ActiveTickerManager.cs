using System.Collections.Concurrent;

namespace Stocks.Hub
{
    internal sealed class ActiveTickerManager
    {
        private readonly ConcurrentBag<string> _activeTickers = new ConcurrentBag<string>();    

        public void AddTicker(string ticker)
        {
            if (!_activeTickers.Contains(ticker))
            {
                _activeTickers.Add(ticker);
            }
        }   

        public IReadOnlyCollection<string> GetAllTicker()
        {
            return _activeTickers.ToList().AsReadOnly();
        }
    }
}
