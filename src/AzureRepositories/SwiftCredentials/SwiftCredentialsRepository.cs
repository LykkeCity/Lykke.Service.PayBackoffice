using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.SwiftCredentials;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.SwiftCredentials
{
    public class SwiftCredentialsEntity : TableEntity, ISwiftCredentials
    {
        public string RegulatorId { get; set; }
        public string AssetId { get; set; }
        public string BIC { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string PurposeOfPayment { get; set; }
        public string BankAddress { get; set; }
        public string CompanyAddress { get; set; }
        public string CorrespondentAccount { get; set; }

        public static string GeneratePartitionKey(string regulatorId)
        {
            return regulatorId;
        }

        public static string GenerateRowKey(string assetId)
        {
            return assetId ?? "*";
        }

        public static SwiftCredentialsEntity Create(ISwiftCredentials credentials)
        {
            return new SwiftCredentialsEntity
            {
                PartitionKey = GeneratePartitionKey(credentials.RegulatorId),
                RowKey = GenerateRowKey(credentials.AssetId),
                RegulatorId = credentials.RegulatorId,
                AssetId = credentials.AssetId,
                AccountName = credentials.AccountName,
                AccountNumber = credentials.AccountNumber,
                BIC = credentials.BIC,
                BankAddress = credentials.BankAddress,
                CompanyAddress = credentials.CompanyAddress,
                PurposeOfPayment = credentials.PurposeOfPayment,
                CorrespondentAccount = credentials.CorrespondentAccount
            };
        }
    }

    public class SwiftCredentialsRepository : ISwiftCredentialsRepository
    {
        private readonly INoSQLTableStorage<SwiftCredentialsEntity> _tableStorage;

        public SwiftCredentialsRepository(INoSQLTableStorage<SwiftCredentialsEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task SaveCredentialsAsync(ISwiftCredentials credentials)
        {
            var entity = SwiftCredentialsEntity.Create(credentials);

            return _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task<IEnumerable<ISwiftCredentials>> GetAllAsync()
        {
            return await _tableStorage.GetDataAsync();
        }

        public async Task<ISwiftCredentials> GetCredentialsAsync(string regulatorId, string assetId = null)
        {
            var partitionKey = SwiftCredentialsEntity.GeneratePartitionKey(regulatorId);
            var rowKey = SwiftCredentialsEntity.GenerateRowKey(assetId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public Task DeleteCredentialsAsync(string regulatorId, string assetId)
        {
            var partitionKey = SwiftCredentialsEntity.GeneratePartitionKey(regulatorId);
            var rowKey = SwiftCredentialsEntity.GenerateRowKey(assetId);

            return _tableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
