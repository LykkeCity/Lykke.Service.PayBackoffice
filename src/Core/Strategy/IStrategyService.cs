using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Strategy
{
    public interface IStrategy
    {
        string AssetPairId { get; set; }
        string Type { get; set; }
    }

    public class Strategy : IStrategy
    {
        public string AssetPairId { get; set; }
        public string Type { get; set; }
    }

    public interface IAveragePriceMovement
    {
        string AssetPairId { get; set; }        
        decimal AskIncrementSpread { get; set; }
        decimal BaseSpread { get; set; }
        decimal BidIncrementSpread { get; set; }
        decimal RelaxationTime { get; set; }
    }

    public interface IEditAveragePriceMovement
    {
        string AssetPairId { get; set; }
        string AskIncrementSpread { get; set; }
        string BaseSpread { get; set; }
        string BidIncrementSpread { get; set; }
        string RelaxationTime { get; set; }
    }

    public class AveragePriceMovement : IAveragePriceMovement
    {
        public string AssetPairId { get; set; }
        public decimal AskIncrementSpread { get; set; }
        public decimal BaseSpread { get; set; }
        public decimal BidIncrementSpread { get; set; }
        public decimal RelaxationTime { get; set; }
    }

    public interface IMarkUp
    {
        string AssetPairId { get; set; }
        string Type { get; set; }
        decimal AskMarkUp { get; set; }
        decimal BidMarkUp { get; set; }
    }

    public interface IEditMarkUp
    {
        string AssetPairId { get; set; }
        string Type { get; set; }
        string AskMarkUp { get; set; }
        string BidMarkUp { get; set; }
    }

    public class MarkUp : IMarkUp
    {
        public string AssetPairId { get; set; }
        public string Type { get; set; }
        public decimal AskMarkUp { get; set; }
        public decimal BidMarkUp { get; set; }
    }

    public interface IStrategyService
    {
        Task<IEnumerable<IStrategy>> GetStrategies();
        Task<IAveragePriceMovement> GetAveragePriceMovement(string assetPairId);
        Task EditAveragePriceMovement(string userId, string assetPairId, IEditAveragePriceMovement averagePriceMovement);
        Task<IMarkUp> GetMarkUp(string assetPairId);
        Task EditMarkUp(string userId, string assetPairId, IEditMarkUp markUp);
    }
}
