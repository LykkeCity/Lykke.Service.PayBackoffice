using System;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.EventLogs;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.EventLogs
{
    public class TempDataRecordEntity : TableEntity
    {
        public static string GeneratePartition(Type t)
        {
            return t.Name;
        }

        public static string GenerateRowKey(string id = null)
        {
            return id ?? "no_id";
        }

        public static TempDataRecordEntity Create(BaseTempData data, string id = null)
        {
            return new TempDataRecordEntity
            {
                PartitionKey = GeneratePartition(data.GetType()),
                RowKey = GenerateRowKey(id),
                Data = data.ToJson()
            };
        }
        
        public string Data { get; set; }
    }

    public class TempDataRepository : ITempDataRepository
    {
        private readonly INoSQLTableStorage<TempDataRecordEntity> _tableStorage;

        public TempDataRepository(INoSQLTableStorage<TempDataRecordEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task InsertOrReplaceDataAsync(BaseTempData data, string id = null)
        {
            var entity = TempDataRecordEntity.Create(data, id);
            return _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task<T> RetrieveData<T>(string id = null) where T : BaseTempData
        {
            string dataJson;
            if (id != null)
            {
                dataJson = (await _tableStorage.GetDataAsync(TempDataRecordEntity.GeneratePartition(typeof (T)),
                    TempDataRecordEntity.GenerateRowKey(id)))?.Data;
            }
            else
            {
                dataJson =
                    (await _tableStorage.GetDataAsync(TempDataRecordEntity.GeneratePartition(typeof (T))))
                        .FirstOrDefault()?.Data;
            }

            return dataJson?.DeserializeJson<T>();
        }
    }
}
