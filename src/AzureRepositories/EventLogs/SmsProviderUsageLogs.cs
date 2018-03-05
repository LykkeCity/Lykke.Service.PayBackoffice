using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.EventLogs;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.EventLogs
{
    public class SmsProviderUsageRecordEntity : TableEntity, ISmsProviderUsageRecord
    {
        public static string GeneratePartition(string provider)
        {
            return provider;
        }

        public static SmsProviderUsageRecordEntity Create(ISmsProviderUsageRecord record)
        {
            return new SmsProviderUsageRecordEntity
            {
                PartitionKey = GeneratePartition(record.Provider),
                Country = record.Country,
                DateTime = record.DateTime,
                Provider = record.Provider,
                PhoneNumber = record.PhoneNumber
            };
        }

        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string Provider { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class SmsProviderUsageLogs : ISmsProviderUsageLogs
    {
        private readonly INoSQLTableStorage<SmsProviderUsageRecordEntity> _tableStorage;

        public SmsProviderUsageLogs(INoSQLTableStorage<SmsProviderUsageRecordEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task InsertRecord(ISmsProviderUsageRecord record)
        {
            var entity = SmsProviderUsageRecordEntity.Create(record);
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity, entity.DateTime);
        }
    }
}
