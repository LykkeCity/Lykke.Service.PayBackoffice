using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.BitCoin;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Bitcoin
{
    public class WalletsGenerationRecord : TableEntity
    {
        public string ClientId { get; set; }
        public string MultiSig { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Disactivated { get; set; }

        public static string GenerateRowKey(string multisig)
        {
            return multisig;
        }

        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }

        public static WalletsGenerationRecord Create(string clientId, string multiSig)
        {
            return new WalletsGenerationRecord
            {
                RowKey = GenerateRowKey(multiSig),
                PartitionKey = GeneratePartitionKey(clientId),
                ClientId = clientId,
                MultiSig = multiSig,
                Created = DateTime.UtcNow
            };
        }
    }

    public class WalletsGenerationHistory : IWalletsGenerationHistory
    {
        private readonly INoSQLTableStorage<WalletsGenerationRecord> _tableStorage;

        public WalletsGenerationHistory(INoSQLTableStorage<WalletsGenerationRecord> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task AddWalletGenerationRecord(string clientId, string multiSig)
        {
            var entity = WalletsGenerationRecord.Create(clientId, multiSig);
            return _tableStorage.InsertAsync(entity);
        }

        public async Task AddWalletDisactivatedRecord(string clientId, string multiSig)
        {
            var entity = await _tableStorage.GetDataAsync(WalletsGenerationRecord.GeneratePartitionKey(clientId),
                WalletsGenerationRecord.GenerateRowKey(multiSig));

            if (entity == null)
            {
                entity = WalletsGenerationRecord.Create(clientId, multiSig);
                entity.Created = null;
            }

            entity.Disactivated = DateTime.UtcNow;

            await _tableStorage.InsertOrMergeAsync(entity);
        }
    }
}
