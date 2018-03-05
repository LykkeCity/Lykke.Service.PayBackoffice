using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Messages.Sms;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Sms
{
    public class SmsMessageMockEntity : TableEntity, ISmsMockRecord
    {
        public static string GeneratePartitionKey(string phoneNumber)
        {
            return phoneNumber;
        }

        public string Id => RowKey;

        public string PhoneNumber => PartitionKey;

        public DateTime DateTime { get; set; }

        public string From { get; set; }

        public string Text { get; set; }

        public static SmsMessageMockEntity Create(string phoneNumber, SmsMessage smsMessage)
        {
            return new SmsMessageMockEntity
            {
                PartitionKey = GeneratePartitionKey(phoneNumber),
                RowKey = Guid.NewGuid().ToString(),
                DateTime = DateTime.UtcNow,
                Text = smsMessage.Text,
                From = smsMessage.From
            };
        }
    }

    public class SmsMockRepository : ISmsMockRepository
    {
        private readonly INoSQLTableStorage<SmsMessageMockEntity> _tableStorage;

        public SmsMockRepository(INoSQLTableStorage<SmsMessageMockEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task InsertAsync(string phoneNumber, SmsMessage msg)
        {
            var newEntity = SmsMessageMockEntity.Create(phoneNumber, msg);
            return _tableStorage.InsertAsync(newEntity);
        }

        public async Task<IEnumerable<ISmsMockRecord>> GetAllAsync()
        {
            return await _tableStorage.GetDataAsync();
        }

        public async Task<IEnumerable<ISmsMockRecord>> Get(string email)
        {
            var partitionKey = SmsMessageMockEntity.GeneratePartitionKey(email);
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<ISmsMockRecord> GetAsync(string email, string id)
        {
            var partitionKey = SmsMessageMockEntity.GeneratePartitionKey(email);

            return await _tableStorage.GetDataAsync(partitionKey, id);
        }
    }
}
