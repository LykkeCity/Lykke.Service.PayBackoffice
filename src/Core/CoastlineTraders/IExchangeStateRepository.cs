using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.CoastlineTraders
{
    public interface IExchangeState
    {
        Guid OrderId { get; set; }
        string OrderExternalId { get; set; }

        decimal OrderPrice { get; set; }
        decimal OrderVolume { get; set; }
        string OrderTradeType { get; set; }
        string OrderOrderType { get; set; }

        string InstrumentName { get; set; }
        string InstrumentExchange { get; set; }
        string Instrument { get; set; }

        //string Order { get; set; }

        string CoastlineTraderName { get; set; }
        string Mode { get; set; }
        double? ConfigurationOriginalDelta { get; set; }

        decimal? CrossRate { get; set; }
    }

    public interface IExchangeStateRepository
    {
        Task<IEnumerable<IExchangeState>> GetExchangesStatesAsync();
    }
}
