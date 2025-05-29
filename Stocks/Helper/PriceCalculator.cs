using Stocks.Models;
using System;

namespace Stocks.RealTime.Api.Helper
{
    public static class PriceCalculator
    {
        public static decimal CalculateNewPrice(decimal basePrice, double maxChangePercent)
        {
            var rand = new Random();
            var priceFactor = (decimal)(rand.NextDouble() * maxChangePercent * 2 - maxChangePercent);
            var priceChange = basePrice * priceFactor;
            return Math.Round(Math.Max(0, basePrice + priceChange), 2);
        }
    }
}
