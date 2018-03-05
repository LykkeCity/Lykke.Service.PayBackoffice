using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.CoastlineTraders;

namespace AzureRepositories.CoastlineTraders
{
    public class ExchangeStateDataEntity : BaseEntity, IExchangeState
    {
        public Guid OrderId { get; set; }
        public string OrderExternalId { get; set; }

        public decimal OrderPrice { get; set; }
        public decimal OrderVolume { get; set; }
        public string OrderTradeType { get; set; }
        public string OrderOrderType { get; set; }

        public string InstrumentName { get; set; }
        public string InstrumentExchange { get; set; }
        public string Instrument { get; set; }

        //public string Order { get; set; }

        public string CoastlineTraderName { get; set; }
        public string Mode { get; set; }
        public double? ConfigurationOriginalDelta { get; set; }

        public decimal? CrossRate { get; set; }
    }

    public class ExchangeStateRepository : IExchangeStateRepository
    {
        private readonly INoSQLTableStorage<ExchangeStateDataEntity> _tableStorage;

        public ExchangeStateRepository(INoSQLTableStorage<ExchangeStateDataEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IExchangeState>> GetExchangesStatesAsync()
        {
            return await _tableStorage.GetDataAsync();
        }
    }
}
