using System;

namespace Core.MarginTrading.MarketMaker
{
    public interface IExternalExchange
    {
        string Id { get; }
        string Name { get; }
        DateTime Timestamp { get; }
    }
}
