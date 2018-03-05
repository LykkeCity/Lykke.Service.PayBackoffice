using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common.Cache;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class BannedClientEntity : TableEntity
    {
        public static string GeneratePartition()
        {
            return "Ban";
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }

        public static BannedClientEntity Create(string clientId)
        {
            return new BannedClientEntity
            {
                PartitionKey = GeneratePartition(),
                RowKey = GenerateRowKey(clientId)
            };
        }

        public string ClientId => RowKey;
    }

    public class BannedClientsRepository : IBannedClientsRepository
    {
        private readonly ICacheManager _cacheManager;
        private readonly INoSQLTableStorage<BannedClientEntity> _tableStorage;

        private const string BannedClientsCacheKey = "clients.banned-list";

        public BannedClientsRepository(ICacheManager cacheManager,
            INoSQLTableStorage<BannedClientEntity> tableStorage)
        {
            _cacheManager = cacheManager;
            _tableStorage = tableStorage;
        }

        public Task BanClient(string clientId)
        {
            _cacheManager.Remove(BannedClientsCacheKey);
            return _tableStorage.InsertOrReplaceAsync(BannedClientEntity.Create(clientId));
        }

        public Task UnBanClient(string clientId)
        {
            _cacheManager.Remove(BannedClientsCacheKey);
            return _tableStorage.DeleteAsync(BannedClientEntity.GeneratePartition(),
                BannedClientEntity.GenerateRowKey(clientId));
        }

        public async Task<IEnumerable<string>> GetBannedClients(IEnumerable<string> clientIds = null)
        {
            if (clientIds != null)
            {
                return (await _tableStorage.GetDataAsync(BannedClientEntity.GeneratePartition(), clientIds.Select(_ => BannedClientEntity.GenerateRowKey(_)))).Select(x => x.ClientId);
            }
            else
            {
                return (await _tableStorage.GetDataAsync(BannedClientEntity.GeneratePartition())).Select(x => x.ClientId);
            }
        }

        public async Task<bool> IsClientBannedWithCache(string clientId, int cacheMinutes)
        {
            var bannedIds = await _cacheManager.Get(BannedClientsCacheKey, cacheMinutes,
                async () =>
                    (await _tableStorage.GetDataAsync(BannedClientEntity.GeneratePartition())).Select(x => x.ClientId));

            return bannedIds.Contains(clientId);
        }

        public async Task<bool> IsClientBanned(string clientId)
        {
            return await _tableStorage.GetDataAsync(BannedClientEntity.GeneratePartition(),
                BannedClientEntity.GenerateRowKey(clientId)) != null;
        }
    }
}
