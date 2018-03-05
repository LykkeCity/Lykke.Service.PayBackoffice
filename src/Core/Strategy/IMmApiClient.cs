using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Strategy
{
    public class ApiStrategyRecord
    {
        public string AssetPairId { get; set; }
        public string Type { get; set; }
    }

    public class ApiAveragePriceMovementRecord
    {
        public decimal AskIncrementSpread { get; set; }
        public decimal BaseSpread { get; set; }
        public decimal BidIncrementSpread { get; set; }
        public decimal RelaxationTime { get; set; }
    }

    public class ApiMarkUpRecord
    {
        public string Type { get; set; }
        public decimal AskMarkUp { get; set; }
        public decimal BidMarkUp { get; set; }
    }

    public interface IMmApiClient
    {
        Task<IEnumerable<ApiStrategyRecord>> GetStrategies();
        Task<ApiAveragePriceMovementRecord> GetAveragePriceMovement(string assetPairId);
        Task EditAveragePriceMovement(string assetPairId, ApiAveragePriceMovementRecord averagePriceMovement);
        Task<ApiMarkUpRecord> GetMarkUp(string assetPairId);
        Task EditMarkUp(string assetPairId, ApiMarkUpRecord markUp);
    }
}
