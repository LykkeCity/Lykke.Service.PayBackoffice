﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.PaymentSystems;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.PaymentSystems
{

    public class PaymentTransactionLogEventEntity : TableEntity, IPaymentTransactionLogEvent
    {
        internal static string GeneratePartitionKey(string transactionId)
        {
            return transactionId;
        }


        public string PaymentTransactrionId => PartitionKey;
        public DateTime DateTime { get; set; }
        public string TechData { get; set; }
        public string Message { get; set; }
        public string Who { get; set; }


        public static PaymentTransactionLogEventEntity Create(IPaymentTransactionLogEvent src)
        {
            return new PaymentTransactionLogEventEntity
            {
                PartitionKey = GeneratePartitionKey(src.PaymentTransactrionId),
                DateTime = src.DateTime,
                Message = src.Message,
                TechData = src.TechData,
                Who = src.Who
            };
        }

    }

    public class PaymentTransactionEventsLog : IPaymentTransactionEventsLog
    {
        private readonly INoSQLTableStorage<PaymentTransactionLogEventEntity> _tableStorage;

        public PaymentTransactionEventsLog(INoSQLTableStorage<PaymentTransactionLogEventEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task WriteAsync(IPaymentTransactionLogEvent newEvent)
        {
            var newEntity = PaymentTransactionLogEventEntity.Create(newEvent);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, newEntity.DateTime);
        }

        public async Task<IEnumerable<IPaymentTransactionLogEvent>> GetAsync(string id)
        {
            var partitionKey = PaymentTransactionLogEventEntity.GeneratePartitionKey(id);
            return await _tableStorage.GetDataAsync(partitionKey);
        }
    }
}
