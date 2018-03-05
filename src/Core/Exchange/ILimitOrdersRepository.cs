using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Assets;

namespace Core.Exchange
{
    public class MatchedOrder
    {
        public string Id { get; set; }
        public double Volume { get; set; }

        internal static MatchedOrder Create(IOrderBase orderBase, double volume)
        {
            return new MatchedOrder
            {
                Id = orderBase.Id,
                Volume = volume
            };
        }
    }


    public enum LimitOrderStatus
    {
        //Init status, limit order in order book
        InOrderBook
        //Partially matched
        , Processing
        //Fully matched
        , Matched
        //Not enough funds on account
        , NotEnoughFunds
        //Reserved volume greater than balance
        , ReservedVolumeGreaterThanBalance
        //No liquidity
        , NoLiquidity
        //Unknown asset
        , UnknownAsset
        //One of trades or whole order has volume/price*volume less then configured dust
        , Dust
        //Cancelled
        , Cancelled
        // negative spread 
        , LeadToNegativeSpread
    }

    public interface ILimitOrder : IOrderBase
    {
        double RemainingVolume { get; set; }
        string MatchingId { get; set; }
    }

    public class LimitOrder : ILimitOrder
    {
        public DateTime CreatedAt { get; set; }
        public DateTime MatchedAt { get; set; }
        public string Id { get; set; }
        public List<MatchedOrder> MatchedOrders { get; set; }
        public string ClientId { get; set; }
        public string BaseAsset { get; set; }
        public string AssetPairId { get; set; }
        public string Status { get; set; }
        public bool Straight { get; set; }
        public OrderAction OrderAction { get; set; }
        public string BlockChain { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public double RemainingVolume { get; set; }
        public string MatchingId { get; set; }

        public static LimitOrder Create(string id, string clientId, string assetPairId, double volume, double price, double remainigVolume)
        {
            return new LimitOrder
            {
                Id = id,
                ClientId = clientId,
                AssetPairId = assetPairId,
                Volume = volume,
                Price = price,
                RemainingVolume = remainigVolume,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public interface ILimitOrdersRepository
    {
        Task<ILimitOrder> GetOrderAsync(string orderId);

        Task CreateAsync(ILimitOrder limitOrder);
        Task RemoveAsync(string id, string clientId);
        Task FinishAsync(ILimitOrder order, string status);

        Task<IEnumerable<ILimitOrder>> GetActiveOrdersAsync(string clientId);
        Task<IEnumerable<ILimitOrder>> GetOrdersAsync(IEnumerable<string> orderIds);
        Task<IEnumerable<ILimitOrder>> GetOrdersAsync(string clientId);
        
    }
}
