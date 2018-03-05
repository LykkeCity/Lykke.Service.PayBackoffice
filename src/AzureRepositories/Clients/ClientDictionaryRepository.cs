using System;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class KeyValueEntity : TableEntity, IKeyValue
    {
        public string ClientId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string Key
        {
            get { return RowKey; }
            set { RowKey = value; }
        }
        public string Value { get; set; }

        public static KeyValueEntity Create(string clientId, IKeyValue keyValue)
        {
            return new KeyValueEntity
            {
                RowKey = keyValue.Key,
                PartitionKey = clientId,
                Value = keyValue.Value
            };
        }
    }

    public class ClientDictionaryRepository : IClientDictionaryRepository
    {
        private readonly INoSQLTableStorage<KeyValueEntity> _tableStorage;

        public ClientDictionaryRepository(INoSQLTableStorage<KeyValueEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task SaveAsync(string clientId, IKeyValue keyValue)
        {
            return _tableStorage.ModifyOrCreateAsync(clientId, keyValue.Key, () => KeyValueEntity.Create(clientId, keyValue),
                entity => entity.Value = keyValue.Value);
        }

        public Task RemoveAsync(string clientId, string key)
        {
            return _tableStorage.DeleteAsync(clientId, key);
        }

        public async Task<IKeyValue> GetAsync(string clientId, string key)
        {
            var entity = await _tableStorage.GetDataAsync(clientId, key);
            return null == entity ? null : new KeyValue {Key = entity.Key, Value = entity.Value};
        }

        public async Task<IKeyValue[]> GetAllAsync(string clientId)
        {
            var entities = await _tableStorage.GetDataAsync(clientId);
            // ReSharper disable once CoVariantArrayConversion
            return entities.Select(x => new KeyValue {Key = x.Key, Value = x.Value}).ToArray();
        }
    }
}
