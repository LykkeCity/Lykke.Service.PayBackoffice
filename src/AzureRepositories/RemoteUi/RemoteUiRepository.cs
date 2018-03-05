using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.RemoteUi;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.RemoteUi
{
    public class RemoteUiEntity : TableEntity
    {
        public static string GeneratePartitionKey()
        {
            return "Setup";
        }

        public static string GenerateRowKey()
        {
            return "RemoteUi";
        }

        public string Data { get; set; }

        public RemoteUiData GetData()
        {
            if (Data == null)
                return RemoteUiData.CreateDefault();

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<RemoteUiData>(Data);
            }
            catch (Exception)
            {
                return RemoteUiData.CreateDefault();
            }

        }


        public static RemoteUiEntity Create(RemoteUiData data)
        {
            return new RemoteUiEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                Data = data.ToJson()
            };
        }


    }

    public class RemoteUiRepository : IRemoteUiRepository
    {
        private readonly INoSQLTableStorage<RemoteUiEntity> _tableStorage;

        public RemoteUiRepository(INoSQLTableStorage<RemoteUiEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task SaveAsync(RemoteUiData data)
        {
            var newEntity = RemoteUiEntity.Create(data);
            return _tableStorage.InsertOrReplaceAsync(newEntity);
        }

        public async Task<RemoteUiData> GetAsync()
        {
            var partitionKey = RemoteUiEntity.GeneratePartitionKey();
            var rowKey = RemoteUiEntity.GenerateRowKey();

            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            return entity == null ? RemoteUiData.CreateDefault() : entity.GetData();

        }
    }

}
