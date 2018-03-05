using System;

namespace Core.MarginTrading.MarketMaker
{
    public class ExternalExchange : IExternalExchange
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
