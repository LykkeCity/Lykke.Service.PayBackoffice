using System.Threading.Tasks;
using AzureStorage;
using Core.Accounts.PrivateWallets;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Accounts.PrivateWallets
{
    public class PrivateWalletBackupEntity : TableEntity, IPrivateWalletBackupRecord
    {
        public string WalletAddress { get; set; }
        public string SecurityQuestion { get; set; }
        public string PrivateKeyBackup { get; set; }

        public static string GeneratePartitionKey()
        {
            return "PWBackup";
        }

        public static string GenerateRowKey(string walletAddress)
        {
            return walletAddress;
        }

        public static PrivateWalletBackupEntity Create(IPrivateWalletBackupRecord record)
        {
            return new PrivateWalletBackupEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(record.WalletAddress),
                PrivateKeyBackup = record.PrivateKeyBackup,
                SecurityQuestion = record.SecurityQuestion,
                WalletAddress = record.WalletAddress
            };
        }

    }

    public class PrivateWalletBackupRepository : IPrivateWalletBackupRepository
    {
        private readonly INoSQLTableStorage<PrivateWalletBackupEntity> _tableStorage;

        public PrivateWalletBackupRepository(INoSQLTableStorage<PrivateWalletBackupEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task SaveBackupAsync(IPrivateWalletBackupRecord privateWalletBackupRecord)
        {
            var entity = PrivateWalletBackupEntity.Create(privateWalletBackupRecord);
            return _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
