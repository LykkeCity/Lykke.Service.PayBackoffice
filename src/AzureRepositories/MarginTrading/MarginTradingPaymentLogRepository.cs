using AzureStorage;
using Core.MarginTrading.Payments;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AzureRepositories.MarginTrading
{
    public class MarginTradingPaymentLogEntity : TableEntity, IMarginTradingPaymentLog
    {
        public string ClientId { get; set; }
        public string AccountId { get; set; }
        public DateTime DateTime { get; set; }
        public double Amount { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }

        internal static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }


        public static MarginTradingPaymentLogEntity Create(IMarginTradingPaymentLog src)
        {
            return new MarginTradingPaymentLogEntity
            {
                PartitionKey = GeneratePartitionKey(src.ClientId),
                Amount = src.Amount,
                AccountId = src.AccountId,
                ClientId = src.ClientId,
                DateTime = src.DateTime,
                Message = src.Message,
                TransactionId = src.TransactionId,
                IsError = src.IsError
            };
        }
        
    }

    public class MarginTradingPaymentLogRepository : IMarginTradingPaymentLogRepository
    {
        private readonly INoSQLTableStorage<MarginTradingPaymentLogEntity> _tableStorage;

        public MarginTradingPaymentLogRepository(INoSQLTableStorage<MarginTradingPaymentLogEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task CreateAsync(IMarginTradingPaymentLog record)
        {
            var newEntity = MarginTradingPaymentLogEntity.Create(record);
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, newEntity.DateTime);
        }
        
        public async Task<IEnumerable<IMarginTradingPaymentLog>> GetByClientAndAccountAsync(string clientId, string accountId, DateTime? dateFrom, DateTime? dateTo)
        {
            var dtFrom = dateFrom.HasValue ? dateFrom.Value : DateTime.MinValue;
            var dtTo = dateTo.HasValue ? dateTo.Value.Date.AddDays(1) : DateTime.Today.AddDays(1);
            if (accountId == null)
                return await _tableStorage.WhereAsync(MarginTradingPaymentLogEntity.GeneratePartitionKey(clientId), dtFrom, dtTo, ToIntervalOption.IncludeTo);
            else
                return await _tableStorage.WhereAsync(MarginTradingPaymentLogEntity.GeneratePartitionKey(clientId), dtFrom, dtTo, ToIntervalOption.IncludeTo, e => e.AccountId == accountId);
        }

        public async Task<IEnumerable<IMarginTradingPaymentLog>> GetByDate(DateTime? dateFrom, DateTime? dateTo)
        {
            var dtFrom = dateFrom.HasValue ? dateFrom.Value : DateTime.MinValue;
            var dtTo = dateTo.HasValue ? dateTo.Value.Date.AddDays(1) : DateTime.Today.AddDays(1);
            return await _tableStorage.WhereAsync(AzureStorageUtils.QueryGenerator<MarginTradingPaymentLogEntity>.RowKeyOnly.BetweenQuery(dtFrom, dtTo, ToIntervalOption.IncludeTo));

        }
       
    }
}
