using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.CoastlineTraders;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.CoastlineTraders
{
    public class ExchangeOperationDataEntity : BaseEntity, IExchangeOperation
    {
        public DateTime CreationTime { get; set; }

        public string OperationType { get; set; }

        public string AssetPairId { get; set; }

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
    }

    public class ExchangeOperationRepository : IExchangeOperationRepository
    {
        private readonly INoSQLTableStorage<ExchangeOperationDataEntity> _tableStorage;

        public ExchangeOperationRepository(INoSQLTableStorage<ExchangeOperationDataEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IExchangeOperation>> GetExchangesOperationsAsync(DateTime date, DateTime timeFrom, DateTime timeTo)
        {
            var dateFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{date:yyyy-MM-dd}");

            var timeFilter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, $"{timeFrom:HH:mm}"),
                TableOperators.And,
                // adding "z" in the end since RowKey comes as $"{utcNow:HH:mm:ss.fffffff}_{Guid.NewGuid():N}"
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, $"{timeTo:HH:mm}z")
            );

            var combinedFilter = TableQuery.CombineFilters(dateFilter, TableOperators.And, timeFilter);

            var query = new TableQuery<ExchangeOperationDataEntity>().Where(combinedFilter);

            return (await _tableStorage.WhereAsync(query)).Reverse();
        }
    }
}
