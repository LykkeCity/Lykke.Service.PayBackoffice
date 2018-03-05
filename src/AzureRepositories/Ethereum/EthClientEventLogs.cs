﻿using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.Ethereum;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Ethereum
{
    public class EthClientEventRecord : TableEntity
    {
        public static EthClientEventRecord Create(string clientId, Event eventType, string data)
        {
            return new EthClientEventRecord
            {
                PartitionKey = clientId,
                Event = eventType,
                Data = data
            };
        }

        public string ClientId => PartitionKey;
        public Event Event { get; set; }
        public string Data { get; set; }
    }

    public class EthClientEventLogs : IEthClientEventLogs
    {
        private readonly INoSQLTableStorage<EthClientEventRecord> _tableStorage;

        public EthClientEventLogs(INoSQLTableStorage<EthClientEventRecord> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task WriteEvent(string clientId, Event eventType, string data)
        {
            var entity = EthClientEventRecord.Create(clientId, eventType, data);
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity, DateTime.UtcNow);
        }
    }
}
