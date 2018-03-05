using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.CoastlineTraders
{
    public interface IExchangeOperation
    {
        DateTime CreationTime { get; set; }

        string OperationType { get; set; }

        string AssetPairId { get; set; }
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
    }

    public interface IExchangeOperationRepository
    {
        Task<IEnumerable<IExchangeOperation>> GetExchangesOperationsAsync(DateTime date, DateTime timeFrom, DateTime timeTo);
    }
}
