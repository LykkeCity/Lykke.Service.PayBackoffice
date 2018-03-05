using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class ClientWithdrawalCacheRepository : IClientWithdrawalCacheRepository
    {
        private readonly INoSQLTableStorage<ClientWithdrawalItem> _tableStorage;

        public ClientWithdrawalCacheRepository(INoSQLTableStorage<ClientWithdrawalItem> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IClientWithdrawalItem> GetAsync(string clientId)
        {
            var partitionKey = ClientWithdrawalItem.GeneratePartitionKey();
            var rowKey = ClientWithdrawalItem.GenerateRowKey(clientId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<bool> TrySaveAsync(string clientId, IClientWithdrawalItem src)
        {
            if (string.IsNullOrWhiteSpace(clientId) || src == null)
                return false;

            var item = ClientWithdrawalItem.Create(src);

            item.PartitionKey = ClientWithdrawalItem.GeneratePartitionKey();
            item.RowKey = ClientWithdrawalItem.GenerateRowKey(clientId);

            await _tableStorage.InsertOrReplaceAsync(item);

            return true;
        }
    }

    public class ClientWithdrawalItem : TableEntity, IClientWithdrawalItem
    {
        public static string GeneratePartitionKey() => "CWC"; // Client Withdrawal Cache
        public static string GenerateRowKey(string clientId) => clientId.Trim().ToLower();

        public static ClientWithdrawalItem Create(IClientWithdrawalItem src)
        {
            return new ClientWithdrawalItem()
            {
                Amount = src.Amount,
                Asset = src.Asset,
                Bic = src.Bic,
                AccNumber = src.AccNumber,
                AccName = src.AccName,
                BankName = src.BankName,
                AccHolderAddress = src.AccHolderAddress,
                AccHolderCity = src.AccHolderCity,
                AccHolderCountry = src.AccHolderCountry,
                AccHolderZipCode = src.AccHolderZipCode
            };
        }

        public double Amount { get; set; }
        public string Asset { get; set; }
        public string Bic { get; set; }
        public string AccNumber { get; set; }
        public string AccName { get; set; }
        public string BankName { get; set; }
        public string AccHolderAddress { get; set; }

        public string AccHolderCountry { get; set; }
        public string AccHolderZipCode { get; set; }
        public string AccHolderCity { get; set; }
    }


}
