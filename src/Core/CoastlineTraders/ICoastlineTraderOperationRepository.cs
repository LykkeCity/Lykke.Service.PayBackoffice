using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.CoastlineTraders
{
    public interface ICoastlineTraderOperation
    {
        DateTime CreationTime { get; }

        string OperationType { get; set; }

        string Name { get; set; }

        string InstrumentName { get; set; }
        string InstrumentExchange { get; set; }
        string InstrumentBase { get; set; }
        string InstrumentQuote { get; set; }

        string Mode { get; set; }
        double ConfigurationOriginalDelta { get; set; }
        int ConfigurationDelayInSeconds { get; set; }
        decimal ConfigurationOriginalUnitSize { get; set; }
        bool MarketOrdersEnabled { get; set; }
        decimal ThresholdFractionToCorrectOrders { get; set; }
        bool IsSleeping { get; set; }
        DateTime? SleepingTo { get; set; }
        decimal AbsoluteInventory { get; }

        string Order { get; set; }
        string OldOderId { get; set; }
        string ExecutedTrade { get; set; }
    }

    public interface ICoastlineTraderOperationRepository
    {
        Task<IEnumerable<ICoastlineTraderOperation>> GetCoastlineTradersOperationsAsync(string coastlineTraderName, DateTime date, DateTime timeFrom, DateTime timeTo);
    }
}
