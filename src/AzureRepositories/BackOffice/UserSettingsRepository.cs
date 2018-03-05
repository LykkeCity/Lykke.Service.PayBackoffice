using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Core.BackOffice;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.BackOffice
{
    public class UserSettingsEntity : TableEntity
    {
        public static string GeneratePartitionKey(string userId)
        {
            return userId;
        }

        public static string GenerateRowKey(string field)
        {
            return field;
        }

        public string Value { get; set; }

        public static UserSettingsEntity Create(string userId, string field, string value)
        {
            return new UserSettingsEntity
            {
                PartitionKey = GeneratePartitionKey(userId),
                RowKey = GenerateRowKey(field),
                Value = value
            };
        }

    }

    public class UserSettingsRepository : IUserSettingsRepository
    {
        private readonly INoSQLTableStorage<UserSettingsEntity> _tableStorage;

        public UserSettingsRepository(INoSQLTableStorage<UserSettingsEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<string> GetAsync(string userId, string field)
        {
            var partitionKey = UserSettingsEntity.GeneratePartitionKey(userId);
            var rowKey = UserSettingsEntity.GenerateRowKey(field);
            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            return entity?.Value;
        }

        public Task SaveAsync(string userId, string field, string value)
        {
            var newEntity = UserSettingsEntity.Create(userId, field, value);
            return _tableStorage.InsertOrReplaceAsync(newEntity);
        }
    }
}
