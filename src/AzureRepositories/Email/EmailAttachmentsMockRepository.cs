using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Messages.Email;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Email
{
    public class EmailAttachmentsMockEntity : TableEntity, IEmailAttachmentsMock
    {
        public string EmailMockId => PartitionKey;
        public string AttachmentFileId => RowKey;
        public string FileName { get; set; }
        public string ContentType { get; set; }

        public static EmailAttachmentsMockEntity Create(string emailMockId, string fileId,
            string fileName, string contentType)
        {
            return new EmailAttachmentsMockEntity
            {
                PartitionKey = GeneratePartition(emailMockId),
                RowKey = GenerateRowKey(fileId),
                FileName = fileName,
                ContentType = contentType
            };
        }

        private static string GenerateRowKey(string fileId)
        {
            return fileId;
        }

        private static string GeneratePartition(string emailMockId)
        {
            return emailMockId;
        }
    }

    public class EmailAttachmentsMockRepository : IEmailAttachmentsMockRepository
    {
        private readonly INoSQLTableStorage<EmailAttachmentsMockEntity> _tableStorage;

        public EmailAttachmentsMockRepository(INoSQLTableStorage<EmailAttachmentsMockEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task InsertAsync(string emailMockId, string fileId, string fileName, string contentType)
        {
            var entity = EmailAttachmentsMockEntity.Create(emailMockId, fileId, fileName, contentType);
            await _tableStorage.InsertAsync(entity);
        }

        public async Task<IEnumerable<IEmailAttachmentsMock>> GetAsync(string emailMockId)
        {
            return await _tableStorage.GetDataAsync(emailMockId);
        }
    }
}
