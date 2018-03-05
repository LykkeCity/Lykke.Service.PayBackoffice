using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Core.Offchain;

namespace AzureRepositories.Offchain
{
    public class OffchainSettingEntity : BaseEntity
    {
        public static string GeneratePartitionKey()
        {
            return "Settings";
        }

        public string Value { get; set; }

        public OffchainSettingEntity(string key, string value)
        {
            RowKey = key;
            Value = value;
            PartitionKey = GeneratePartitionKey();
        }

        public OffchainSettingEntity()
        {

        }
    }

    public class OffchainSettingsRepository : IOffchainSettingsRepository
    {
        private readonly INoSQLTableStorage<OffchainSettingEntity> _table;

        public OffchainSettingsRepository(INoSQLTableStorage<OffchainSettingEntity> table)
        {
            _table = table;
        }
        public Task<T> Get<T>(string key)
        {
            return Get<T>(key, default(T));
        }

        public async Task<T> Get<T>(string key, T defaultValue)
        {
            var setting = await _table.GetDataAsync(OffchainSettingEntity.GeneratePartitionKey(), key);
            if (setting == null)
                return defaultValue;
            return (T)Convert.ChangeType(setting.Value, typeof(T));
        }

        public Task Set<T>(string key, T value)
        {
            return _table.InsertOrReplaceAsync(new OffchainSettingEntity(key, value?.ToString()));
        }
    }
}
