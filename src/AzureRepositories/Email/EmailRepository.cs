using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Messages.Email;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Email
{
    public class EmailMockEntity : TableEntity, ISmtpMailMock
    {
        public static string GeneratePartitionKey(string email)
        {
            return email.ToLower();
        }

        public string Id => RowKey;

        public string Address => PartitionKey;

        public DateTime DateTime { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public bool IsHtml { get; set; }
    }

    public class EmailMockRepository : IEmailMockRepository
    {
        private readonly INoSQLTableStorage<EmailMockEntity> _tableStorage;

        public EmailMockRepository(INoSQLTableStorage<EmailMockEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<ISmtpMailMock>> GetAllAsync()
        {
            return await _tableStorage.GetDataAsync();
        }

        public async Task<IEnumerable<ISmtpMailMock>> Get(string email)
        {
            var partitionKey = EmailMockEntity.GeneratePartitionKey(email);
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<ISmtpMailMock> GetAsync(string email, string id)
        {
            var partitionKey = EmailMockEntity.GeneratePartitionKey(email);

            return await _tableStorage.GetDataAsync(partitionKey, id);
        }
    }
}
