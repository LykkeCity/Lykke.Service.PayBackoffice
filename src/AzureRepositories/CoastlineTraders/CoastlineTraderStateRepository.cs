using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.CoastlineTraders;

namespace AzureRepositories.CoastlineTraders
{
    public class CoastlineTraderStateEntity : BaseEntity, ICoastlineTraderState
    {
        public string Name { get; set; }

        public string InstrumentName { get; set; }

        public string Mode { get; set; }
        public double ConfigurationOriginalDelta { get; set; }
        public int ConfigurationDelayInSeconds { get; set; }
        public decimal ConfigurationOriginalUnitSize { get; set; }
        public bool MarketOrdersEnabled { get; set; }
        public decimal ThresholdFractionToCorrectOrders { get; set; }
        public bool IsSleeping { get; set; }
        public DateTime? SleepingTo { get; set; }
        public decimal AbsoluteInventory { get; set; }

        public decimal? TargetAbsPnL { get; set; }
        public decimal? CurrentPnL { get; set; }
        public decimal? PositionRealizedProfit { get; set; }

        public ICoastlineTraderOrder OrderBuy { get; set; }
        public ICoastlineTraderOrder OrderSell { get; set; }
    }

    public class CoastlineTraderOrderEntity : BaseEntity, ICoastlineTraderOrder
    {
        public string CoastlineName { get; set; }

        public string OrderIntrinsicEvent { get; set; }
        public decimal OrderDelta { get; set; }
        public string OrderId { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal OrderVolume { get; set; }
        public string OrderTradeType { get; set; }
        public string OrderOrderType { get; set; }
    }

    public class CoastlineTraderStateRepository : ICoastlineTraderStateRepository
    {
        private readonly INoSQLTableStorage<CoastlineTraderStateEntity> _coastlineTraderTableStorage;
        private readonly INoSQLTableStorage<CoastlineTraderOrderEntity> _coastlineTraderOrderTableStorage;

        public CoastlineTraderStateRepository(INoSQLTableStorage<CoastlineTraderStateEntity> coastlineTraderTableStorage,
            INoSQLTableStorage<CoastlineTraderOrderEntity> coastlineTraderOrderTableStorage)
        {
            _coastlineTraderTableStorage = coastlineTraderTableStorage;
            _coastlineTraderOrderTableStorage = coastlineTraderOrderTableStorage;
        }

        public async Task<IEnumerable<ICoastlineTraderState>> GetCoastlineTradersStatesAsync()
        {
            return await _coastlineTraderTableStorage.GetDataAsync();
        }

        public async Task<IEnumerable<ICoastlineTraderOrder>> GetCoastlineTradersOrdersAsync()
        {
            return await _coastlineTraderOrderTableStorage.GetDataAsync();
        }
    }
}
