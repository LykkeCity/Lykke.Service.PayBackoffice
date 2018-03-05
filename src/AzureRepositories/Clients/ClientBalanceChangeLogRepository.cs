using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class ClientBalanceChangeLogRecordEntity : TableEntity, IClientBalanceChangeLogRecord
    {
        public string ClientId { get; set; }
        public DateTime TransactionTimestamp { get; set; }
        public string TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Asset { get; set; }
        public double OldBalance { get; set; }
        public double NewBalance { get; set; }

        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }
   }

    public class ClientBalanceChangeLogRepository : IClientBalanceChangeLogRepository
    {
        private readonly INoSQLTableStorage<ClientBalanceChangeLogRecordEntity> _tableStorage;

        public ClientBalanceChangeLogRepository(INoSQLTableStorage<ClientBalanceChangeLogRecordEntity> tableStorage )
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IClientBalanceChangeLogRecord>> GetByClientAsync(string clientId)
        {
            var partitionKey = ClientBalanceChangeLogRecordEntity.GeneratePartitionKey(clientId);

            return await _tableStorage.GetDataAsync(partitionKey);
        }
    }
}
