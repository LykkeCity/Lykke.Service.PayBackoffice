using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.PaymentSystems;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.PaymentSystems
{
    public class PaymentSystemRawLogEventEntity : TableEntity, IPaymentSystemRawLogEvent
    {
        public static string GeneratePartitionKey(string paymentSystem)
        {
            return paymentSystem;
        }

        public DateTime DateTime { get; set; }
        public string PaymentSystem => PartitionKey;
        public string EventType { get; set; }
        public string Data { get; set; }

        public static PaymentSystemRawLogEventEntity Create(IPaymentSystemRawLogEvent src)
        {
            return new PaymentSystemRawLogEventEntity
            {
                PartitionKey = GeneratePartitionKey(src.PaymentSystem),
                DateTime = src.DateTime,
                Data = src.Data,
                EventType = src.EventType
            };
        }

    }

    public class PaymentSystemsRawLog : IPaymentSystemsRawLog
    {
        private readonly INoSQLTableStorage<PaymentSystemRawLogEventEntity> _tableStorage;

        public PaymentSystemsRawLog(INoSQLTableStorage<PaymentSystemRawLogEventEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task RegisterEventAsync(IPaymentSystemRawLogEvent evnt)
        {
            var newEntity = PaymentSystemRawLogEventEntity.Create(evnt);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, evnt.DateTime);

        }
    }
}
