using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Settings;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Settings
{
    public class LkkSourceWalletEntity : TableEntity, ILkkSourceWallet
    {
        public static string GeneratePartition()
        {
            return "LkkSourceWallet";
        }

        public static string GenerateRowKey(string address)
        {
            return address;
        }

        public static LkkSourceWalletEntity Create(ILkkSourceWallet wallet)
        {
            return new LkkSourceWalletEntity
            {
                Address = wallet.Address,
                Comment = wallet.Comment,
                StartBalance = wallet.StartBalance,
                PartitionKey = GeneratePartition(),
                RowKey = GenerateRowKey(wallet.Address)
            };
        }

        public string Address { get; set; }
        public double StartBalance { get; set; }
        public string Comment { get; set; }
    }

    public class LkkSourceWalletRepository : ILkkSourceWalletsRepository
    {
        private readonly INoSQLTableStorage<LkkSourceWalletEntity> _tableStorage;

        public LkkSourceWalletRepository(INoSQLTableStorage<LkkSourceWalletEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task InsertOrReplaceAsync(ILkkSourceWallet lkkSourceWallet)
        {
            var entity = LkkSourceWalletEntity.Create(lkkSourceWallet);
            return _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task<ILkkSourceWallet> GetRecord(string address)
        {
            return await _tableStorage.GetDataAsync(LkkSourceWalletEntity.GeneratePartition(),
                LkkSourceWalletEntity.GenerateRowKey(address));
        }

        public async Task<IEnumerable<ILkkSourceWallet>> GetRecordsAsync()
        {
            return await _tableStorage.GetDataAsync(LkkSourceWalletEntity.GeneratePartition());
        }

        public Task RemoveAsync(string address)
        {
            return _tableStorage.DeleteAsync(LkkSourceWalletEntity.GeneratePartition(),
                LkkSourceWalletEntity.GenerateRowKey(address));
        }
    }
}
