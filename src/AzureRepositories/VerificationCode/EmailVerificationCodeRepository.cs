using System;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.VerificationCode;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.VerificationCode
{
    public class EmailVerificationCodeEntity : TableEntity, IEmailVerificationCode
    {
        public string Id => RowKey;
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime CreationDateTime { get; set; }

        public static string GenerateRowKey(DateTime creationDt)
        {
            return IdGenerator.GenerateDateTimeIdNewFirst(creationDt);
        }

        public static string GeneratePartitionKey(string email, string partnerId)
        {

            return string.IsNullOrEmpty(partnerId) ? $"{email}" : $"{email}_{partnerId}";
        }

        protected static string GenerateRandomCode()
        {
            var rand = new Random(DateTime.UtcNow.Millisecond);
            return rand.Next(9999).ToString("0000");
        }

        public static EmailVerificationCodeEntity Create(string email, DateTime creationDt, bool generateRealCode, string partnerId = null)
        {
            return new EmailVerificationCodeEntity
            {
                RowKey = GenerateRowKey(creationDt),
                PartitionKey = GeneratePartitionKey(email, partnerId),
                Code = generateRealCode ? GenerateRandomCode() : "0000",
                Email = email,
                CreationDateTime = creationDt
            };
        }
    }

    public class EmailVerificationPriorityCodeEntity : EmailVerificationCodeEntity, IEmailVerificationPriorityCode
    {
        public DateTime ExpirationDate { get; set; }

        public static EmailVerificationPriorityCodeEntity Create(string email, DateTime creationDt, DateTime expirationDt, string partnerId = null)
        {
            return new EmailVerificationPriorityCodeEntity
            {
                RowKey = GenerateRowKey(creationDt),
                PartitionKey = GeneratePartitionKey(email, partnerId),
                Code = GenerateRandomCode(),
                Email = email,
                CreationDateTime = creationDt,
                ExpirationDate = expirationDt
            };
        }
    }

    public class EmailVerificationCodeRepository : IEmailVerificationCodeRepository
    {
        private readonly INoSQLTableStorage<EmailVerificationCodeEntity> _tableStorage;
        private readonly INoSQLTableStorage<EmailVerificationPriorityCodeEntity> _priorityCodesStorage;

        public EmailVerificationCodeRepository(INoSQLTableStorage<EmailVerificationCodeEntity> tableStorage,
            INoSQLTableStorage<EmailVerificationPriorityCodeEntity> priorityCodesStorage)
        {
            _tableStorage = tableStorage;
            _priorityCodesStorage = priorityCodesStorage;
        }

        public async Task<IEmailVerificationCode> CreateAsync(string email, string partnerId, bool generateRealCode)
        {
            var entity = EmailVerificationCodeEntity.Create(email, DateTime.UtcNow, generateRealCode, partnerId);
            await _tableStorage.InsertAsync(entity);
            return entity;
        }

        public async Task<IEmailVerificationPriorityCode> CreatePriorityAsync(string email, string partnerId, DateTime expirationDt)
        {
            var entity = EmailVerificationPriorityCodeEntity.Create(email, DateTime.UtcNow, expirationDt, partnerId);
            await _priorityCodesStorage.InsertAsync(entity);
            return entity;
        }

        public async Task<IEmailVerificationCode> GetActualCode(string email, string partnerId)
        {
            var priorityCode =
                await _priorityCodesStorage.GetTopRecordAsync(
                    EmailVerificationCodeEntity.GeneratePartitionKey(email, partnerId));

            if (priorityCode == null || priorityCode.ExpirationDate < DateTime.UtcNow)
            {
                return await _tableStorage.GetTopRecordAsync(EmailVerificationCodeEntity.GeneratePartitionKey(email, partnerId));
            }

            return priorityCode;
        }

        public async Task<bool> CheckAsync(string email,  string partnerId, string codeToCheck)
        {
            var actualCode = await GetActualCode(email, partnerId);
            return actualCode != null && actualCode.Code == codeToCheck;
        }

        public async Task DeleteCodesByEmailAsync(string email, string partnerId)
        {
            await Task.WhenAll(
                DeleteCodesInRepository(email, partnerId, _tableStorage),
                DeleteCodesInRepository(email, partnerId, _priorityCodesStorage)
            );
        }

        private static async Task DeleteCodesInRepository<T>(string email, string partnerId, INoSQLTableStorage<T> storage) where T : ITableEntity, new()
        {
            var batchOperation = new TableBatchOperation();
            var entitiesToDelete = (await storage.GetDataAsync(EmailVerificationCodeEntity.GeneratePartitionKey(email, partnerId))).ToArray();
            if (entitiesToDelete.Any())
            {
                foreach (var e in entitiesToDelete)
                    batchOperation.Delete(e);
                await storage.DoBatchAsync(batchOperation);
            }
        }
    }
}
