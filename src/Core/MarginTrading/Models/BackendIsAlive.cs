namespace Core.MarginTrading.Models
{
    public class BackendIsAlive : IBackendIsAlive
    {
        public bool MatchingEngineAlive { get; set; }
        public bool TradingEngineAlive { get; set; }
        public string Version { get; set; }
        public string Env { get; set; }
        public System.DateTime ServerTime { get; set; }
    }
}
