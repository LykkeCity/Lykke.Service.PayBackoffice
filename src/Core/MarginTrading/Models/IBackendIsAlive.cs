using System;

namespace Core.MarginTrading.Models
{
    public interface IBackendIsAlive
    {
        string Env { get; set; }
        bool MatchingEngineAlive { get; set; }
        DateTime ServerTime { get; set; }
        bool TradingEngineAlive { get; set; }
        string Version { get; set; }
    }
}
