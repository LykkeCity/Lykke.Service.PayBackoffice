using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.CoastlineTraders;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.CoastlineTraders
{
    public class CoastlineTraderOperationDataEntity : BaseEntity, ICoastlineTraderOperation
    {
        public DateTime CreationTime { get; set; }

        public string OperationType { get; set; }

        public string Name { get; set; }

        public string InstrumentName { get; set; }
        public string InstrumentExchange { get; set; }
        public string InstrumentBase { get; set; }
        public string InstrumentQuote { get; set; }

        public string Mode { get; set; }
        public double ConfigurationOriginalDelta { get; set; }
        public int ConfigurationDelayInSeconds { get; set; }
        public decimal ConfigurationOriginalUnitSize { get; set; }
        public bool MarketOrdersEnabled { get; set; }
        public decimal ThresholdFractionToCorrectOrders { get; set; }
        public bool IsSleeping { get; set; }
        public DateTime? SleepingTo { get; set; }
        public decimal AbsoluteInventory { get; set; }

        public string Order { get; set; }
        public string OldOderId { get; set; }
        public string ExecutedTrade { get; set; }
    }

    public class CoastlineTraderOperationRepository : ICoastlineTraderOperationRepository
    {
        private readonly INoSQLTableStorage<CoastlineTraderOperationDataEntity> _tableStorage;

        public CoastlineTraderOperationRepository(INoSQLTableStorage<CoastlineTraderOperationDataEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<ICoastlineTraderOperation>> GetCoastlineTradersOperationsAsync(string coastlineTraderName, DateTime date, DateTime timeFrom, DateTime timeTo)
        {
            var dateFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{date:yyyy-MM-dd}");

            var timeFilter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, $"{timeFrom:HH:mm}"),
                TableOperators.And,
                // adding "z" in the end since RowKey comes as $"{utcNow:HH:mm:ss.fffffff}_{Guid.NewGuid():N}"
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, $"{timeTo:HH:mm}z")
            );

            var combinedFilter = TableQuery.CombineFilters(dateFilter, TableOperators.And, timeFilter);

            string finalFilter;
            if (!string.IsNullOrWhiteSpace(coastlineTraderName))
            {
                var nameFilter = TableQuery.GenerateFilterCondition("Name", QueryComparisons.Equal, coastlineTraderName);

                finalFilter = TableQuery.CombineFilters(combinedFilter, TableOperators.And, nameFilter);
            }
            else
            {
                finalFilter = combinedFilter;
            }

            var query = new TableQuery<CoastlineTraderOperationDataEntity>().Where(finalFilter);

            return (await _tableStorage.WhereAsync(query)).Reverse();
        }
    }
}
