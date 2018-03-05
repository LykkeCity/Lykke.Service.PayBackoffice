using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.MmSettings;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.MmSettings
{
    public class MmSettingsAuditLogDataEntity : TableEntity, IMmSettingsAuditLogData
    {
        public static string GeneratePartitionKey(DateTime createdTime)
        {
            return $"{createdTime:yyyy-MM-dd HH:mm}";
        }

        public static string GenerateRowKey(string userId, DateTime createdTime)
        {
            return $"{userId}_{IdGenerator.GenerateDateTimeIdNewFirst(createdTime)}";
        }

        public string AssetPairId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedTime { get; set; }
        public MmSettingsAuditRecordType RecordType { get; set; }
        public string BeforeJson { get; set; }
        public string AfterJson { get; set; }

        public static MmSettingsAuditLogDataEntity Create(IMmSettingsAuditLogData data)
        {
            return new MmSettingsAuditLogDataEntity
            {
                PartitionKey = GeneratePartitionKey(data.CreatedTime),
                RowKey = GenerateRowKey(data.UserId, data.CreatedTime),
                AssetPairId = data.AssetPairId,
                UserId = data.UserId,
                CreatedTime = data.CreatedTime,
                RecordType = data.RecordType,
                BeforeJson = data.BeforeJson,
                AfterJson = data.AfterJson
            };
        }
    }

    public class MmSettingsAuditLogRepository : IMmSettingsAuditLogRepository
    {
        private readonly INoSQLTableStorage<MmSettingsAuditLogDataEntity> _tableStorage;

        public MmSettingsAuditLogRepository(INoSQLTableStorage<MmSettingsAuditLogDataEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task InsertRecord(IMmSettingsAuditLogData record)
        {
            await _tableStorage.InsertAsync(MmSettingsAuditLogDataEntity.Create(record));
        }
    }
}
