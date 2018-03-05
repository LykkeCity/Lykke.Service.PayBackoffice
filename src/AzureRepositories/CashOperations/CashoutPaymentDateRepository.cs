using AzureStorage;
using Core.CashOperations;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureRepositories.CashOperations
{
    public class CashoutPaymentDateEntity : TableEntity, ICashoutPaymentDate
    {
        public DateTime PaymentDate { get; set; }
        public string RequestId { get; set; }
    }

    public class CashoutPaymentDateRepository : ICashoutPaymentDateRepository
    {
        readonly INoSQLTableStorage<CashoutPaymentDateEntity> _tableStorage;
        private static string GetPartitionKey() => "CashoutPaymentDate";
        private static string GetRowKey(string requestId) => requestId;

        public CashoutPaymentDateRepository(INoSQLTableStorage<CashoutPaymentDateEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task AddAsync(string requestId, DateTime paymentDate)
        {
            var entity = new CashoutPaymentDateEntity()
            {
                PartitionKey = GetPartitionKey(),
                RowKey = GetRowKey(requestId),
                RequestId = requestId,
                PaymentDate = paymentDate
            };

            try
            {
                await _tableStorage.InsertOrReplaceAsync(entity);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ICashoutPaymentDate> GetAsync(string requestId)
        {
            return await _tableStorage.GetDataAsync(GetPartitionKey(), GetRowKey(requestId));
        }
    }
}
