using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.CoastlineTraders
{
    public interface ICoastlineTraderState
    {
        string Name { get; set; }

        string InstrumentName { get; set; }

        string Mode { get; set; }
        double ConfigurationOriginalDelta { get; set; }
        int ConfigurationDelayInSeconds { get; set; }
        decimal ConfigurationOriginalUnitSize { get; set; }
        bool MarketOrdersEnabled { get; set; }
        decimal ThresholdFractionToCorrectOrders { get; set; }
        bool IsSleeping { get; set; }
        DateTime? SleepingTo { get; set; }
        decimal AbsoluteInventory { get; }

        decimal? TargetAbsPnL { get; set; }
        decimal? CurrentPnL { get; set; }
        decimal? PositionRealizedProfit { get; set; }

        ICoastlineTraderOrder OrderBuy { get; set; }
        ICoastlineTraderOrder OrderSell { get; set; }
    }

    public interface ICoastlineTraderOrder
    {
        string CoastlineName { get; set; }

        string OrderIntrinsicEvent { get; set; }
        decimal OrderDelta { get; set; }
        string OrderId { get; set; }
        decimal OrderPrice { get; set; }
        decimal OrderVolume { get; set; }
        string OrderTradeType { get; set; }
        string OrderOrderType { get; set; }
    }

    public interface ICoastlineTraderStateRepository
    {
        Task<IEnumerable<ICoastlineTraderState>> GetCoastlineTradersStatesAsync();
        Task<IEnumerable<ICoastlineTraderOrder>> GetCoastlineTradersOrdersAsync();
    }
}
