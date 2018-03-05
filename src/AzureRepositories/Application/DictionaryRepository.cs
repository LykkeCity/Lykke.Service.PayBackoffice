using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Application;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Application
{
    public class KeyValueEntity : TableEntity, IKeyValue
    {
        public const string PartitionKeyVal = "D";

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
                PartitionKey = PartitionKeyVal,
                Value = keyValue.Value
            };
        }
    }

    public class DictionaryRepository : IDictionaryRepository
    {
        private readonly INoSQLTableStorage<KeyValueEntity> _tableStorage;

        public DictionaryRepository(INoSQLTableStorage<KeyValueEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task SaveAsync(IKeyValue keyValue)
        {
            return _tableStorage.ModifyOrCreateAsync(KeyValueEntity.PartitionKeyVal, keyValue.Key,
                () => KeyValueEntity.Create(KeyValueEntity.PartitionKeyVal, keyValue),
                entity => entity.Value = keyValue.Value);
        }

        public Task RemoveAsync(string key)
        {
            return _tableStorage.DeleteAsync(KeyValueEntity.PartitionKeyVal, key);
        }

        public async Task<IKeyValue> GetAsync(string key)
        {
            var entity = await _tableStorage.GetDataAsync(KeyValueEntity.PartitionKeyVal, key);
            return null == entity ? null : new KeyValue {Key = entity.Key, Value = entity.Value};
        }

        public async Task<IKeyValue[]> GetAllAsync()
        {
            var entities = await _tableStorage.GetDataAsync(KeyValueEntity.PartitionKeyVal);
            // ReSharper disable once CoVariantArrayConversion
            return entities.Select(x => new KeyValue {Key = x.Key, Value = x.Value}).ToArray();
        }
    }
}
