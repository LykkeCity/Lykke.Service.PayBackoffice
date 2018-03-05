using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.BitCoin;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Bitcoin
{
    public class UnsignedTransactionEntity : TableEntity, IUnsignedTransaction
    {
        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public static UnsignedTransactionEntity Create(IUnsignedTransaction transaction)
        {
            return new UnsignedTransactionEntity
            {
                PartitionKey = GeneratePartitionKey(transaction.ClientId),
                RowKey = GenerateRowKey(transaction.Id),
                Id = transaction.Id,
                Hex = transaction.Hex,
                ClientId = transaction.ClientId
            };
        }

        public string Id { get; set; }
        public string Hex { get; set; }
        public string ClientId { get; set; }
    }

    public class UnsignedTransactionsRepository : IUnsignedTransactionsRepository
    {
        private readonly INoSQLTableStorage<UnsignedTransactionEntity> _tableStorage;

        public UnsignedTransactionsRepository(INoSQLTableStorage<UnsignedTransactionEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IUnsignedTransaction>> GetPendingTransactionsAsync(string clientId)
        {
            return await _tableStorage.GetDataAsync(UnsignedTransactionEntity.GeneratePartitionKey(clientId));
        }

        public async Task InsertAsync(IUnsignedTransaction transaction)
        {
            var entity = UnsignedTransactionEntity.Create(transaction);
            await _tableStorage.InsertAsync(entity);
        }

        public Task RemoveAsync(string clientId, string transactionId)
        {
            return _tableStorage.DeleteAsync(clientId, transactionId);
        }
    }
}
