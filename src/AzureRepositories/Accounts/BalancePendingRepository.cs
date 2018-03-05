using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Accounts;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Accounts
{
    public class BalancePendingEntity : TableEntity, IBalancePendingRecord
    {

        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }

        public static string GenerateRowKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string Id => RowKey;
        public string ClientId => PartitionKey;
        public string AssetId { get; set; }
        public double BalancePending { get; set; }
        public string BlockChainHash { get; set; }

        public static BalancePendingEntity Create(string clientId, string assetId, double balance)
        {
            return new BalancePendingEntity
            {
                RowKey = GenerateRowKey(),
                PartitionKey = GeneratePartitionKey(clientId),
                AssetId = assetId,
                BalancePending = balance
            };
        }
    }

    public class BalancePendingRepository : IBalancePendingRepository
    {
        private readonly INoSQLTableStorage<BalancePendingEntity> _tableStorage;

        public BalancePendingRepository(INoSQLTableStorage<BalancePendingEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IBalancePendingRecord>> GetAsync(string clientId)
        {
            var partitionKey = BalancePendingEntity.GeneratePartitionKey(clientId);
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<string> CreateAsync(string clientId, string assetId, double balance)
        {
            var pendingBalanceRecord = BalancePendingEntity.Create(clientId, assetId, balance);

            await _tableStorage.InsertAsync(pendingBalanceRecord);

            return pendingBalanceRecord.Id;
        }

        public Task UpdateBlockchainHashAsync(string clientId, string recordId, string hash)
        {
            var partitionKey = BalancePendingEntity.GeneratePartitionKey(clientId);
            var rowKey = recordId;

            return _tableStorage.MergeAsync(partitionKey, rowKey, entity =>
            {
                entity.BlockChainHash = hash;
                return entity;
            });
        }

        public async Task RemoveByBcnHash(string clientId, string hash)
        {
            var partitionKey = BalancePendingEntity.GeneratePartitionKey(clientId);
            var record = (await _tableStorage.GetDataAsync(partitionKey)).FirstOrDefault(x => x.BlockChainHash == hash);

            if (record != null)
                await _tableStorage.DeleteAsync(record);
        }
    }
}
