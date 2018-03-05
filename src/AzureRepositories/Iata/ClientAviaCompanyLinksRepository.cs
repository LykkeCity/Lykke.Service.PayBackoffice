using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Iata;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Iata
{
    public class ClientAviaCompanyLinkEntity : TableEntity, IClientAviaCompanyLink
    {
        public static string GeneratePartitionKey()
        {
            return "Link";
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }

        public string ClientId => RowKey;

        public string AviaCompanyId { get; set; }

        public static ClientAviaCompanyLinkEntity Create(string clientId, string aviaCompanyId)
        {
            return new ClientAviaCompanyLinkEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(clientId),
                AviaCompanyId = aviaCompanyId
            };
        }
    }

    public class ClientAviaCompanyLinksRepository : IClientAviaCompanyLinksRepository
    {
        private readonly INoSQLTableStorage<ClientAviaCompanyLinkEntity> _tableStorage;

        public ClientAviaCompanyLinksRepository(INoSQLTableStorage<ClientAviaCompanyLinkEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task LinkAsync(string clientId, string aviaCompanyId)
        {
            var newEntity = ClientAviaCompanyLinkEntity.Create(clientId, aviaCompanyId);
            return _tableStorage.InsertOrReplaceAsync(newEntity);
        }

        public async Task<string> GetAviaCompanyId(string clientId)
        {
            var partitionKey = ClientAviaCompanyLinkEntity.GeneratePartitionKey();
            var rowKey = ClientAviaCompanyLinkEntity.GenerateRowKey(clientId);
            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);
            return entity?.AviaCompanyId;
        }

        public Task ClearAsync(string clientId)
        {
            var partitionKey = ClientAviaCompanyLinkEntity.GeneratePartitionKey();
            var rowKey = ClientAviaCompanyLinkEntity.GenerateRowKey(clientId);
            return _tableStorage.DeleteAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IClientAviaCompanyLink>> GetAllAsync()
        {
            var partitionKey = ClientAviaCompanyLinkEntity.GeneratePartitionKey();
            return await _tableStorage.GetDataAsync(partitionKey);
        }
    }
}
