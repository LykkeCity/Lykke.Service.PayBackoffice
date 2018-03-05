using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Users;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Users
{

    public class BackofficeUserRoleEntity : TableEntity, IBackofficeUserRole
    {
        public static string GeneratePartitionKey()
        {
            return "role";
        }

        public static string GenegrateRowKey(string id)
        {
            return id;
        }

        public string Id => RowKey;
        public string Name { get; set; }


        public string Features { get; set; }

        UserFeatureAccess[] IBackofficeUserRole.Features
            => Features.FromStringViaSeparator('|').Select(itm => itm.ParseEnum(UserFeatureAccess.Nothing)).ToArray();

        private void SetFeatures(IEnumerable<UserFeatureAccess> features)
        {
            Features = features.Select(itm => itm.ToString()).ToStringViaSeparator("|");
        }


        public static BackofficeUserRoleEntity Create(IBackofficeUserRole src)
        {
            var result = new BackofficeUserRoleEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = src.Id == null ? Guid.NewGuid().ToString("N") : GenegrateRowKey(src.Id),
                Name = src.Name
            };

            result.SetFeatures(src.Features);

            return result;
        }

    }

    public class BackOfficeUserRolesRepository : IBackofficeUserRolesRepository
    {
        private readonly INoSQLTableStorage<BackofficeUserRoleEntity> _tableStorage;

        public BackOfficeUserRolesRepository(INoSQLTableStorage<BackofficeUserRoleEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IBackofficeUserRole>> GetAllRolesAsync()
        {
            var partitionKey = BackofficeUserRoleEntity.GeneratePartitionKey();
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IBackofficeUserRole> GetAsync(string id)
        {
            var partitionKey = BackofficeUserRoleEntity.GeneratePartitionKey();
            var rowKey = BackofficeUserRoleEntity.GenegrateRowKey(id);
            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task SaveAsync(IBackofficeUserRole data)
        {
            var newEntity = BackofficeUserRoleEntity.Create(data);
            await _tableStorage.InsertOrReplaceAsync(newEntity);
        }
    }
}
