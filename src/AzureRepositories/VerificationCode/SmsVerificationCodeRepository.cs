using System;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.VerificationCode;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.VerificationCode
{
    public class SmsVerificationCodeEntity : TableEntity, ISmsVerificationCode
    {
        public string Id => RowKey;
        public string Phone => PartitionKey;
        public string Code { get; set; }
        public DateTime CreationDateTime { get; set; }

        public static string GenerateRowKey(DateTime creationDt)
        {
            return IdGenerator.GenerateDateTimeIdNewFirst(creationDt);
        }

        public static string GeneratePartitionKey(string partnerId, string phoneNumber)
        {
            var phoneNumberE164 = phoneNumber.ToE164Number();
            if (phoneNumberE164 == null)
                throw new ArgumentException("phoneNumber");

            if(string.IsNullOrWhiteSpace(partnerId))
                return phoneNumberE164;
            return $"{partnerId}_{phoneNumberE164}";
        }

        protected static string GenerateRandomCode()
        {
            var rand = new Random(DateTime.UtcNow.Millisecond);
            return rand.Next(9999).ToString("0000");
        }

        public static SmsVerificationCodeEntity Create(string partnerId, string phone, DateTime creationDt, bool generateRealCode)
        {
            return new SmsVerificationCodeEntity
            {
                RowKey = GenerateRowKey(creationDt),
                PartitionKey = GeneratePartitionKey(partnerId, phone),
                Code = generateRealCode? GenerateRandomCode() : "0000",
                CreationDateTime = creationDt
            };
        }
    }

    public class SmsVerificationPriorityCodeEntity : SmsVerificationCodeEntity, ISmsVerificationPriorityCode
    {
        public DateTime ExpirationDate { get; set; }

        public static SmsVerificationPriorityCodeEntity Create(string partnerId, string phone, DateTime creationDt, DateTime expirationDt)
        {
            return new SmsVerificationPriorityCodeEntity
            {
                RowKey = GenerateRowKey(creationDt),
                PartitionKey = GeneratePartitionKey(partnerId, phone),
                Code = GenerateRandomCode(),
                CreationDateTime = creationDt,
                ExpirationDate = expirationDt
            };
        }
    }

    public class SmsVerificationCodeRepository : ISmsVerificationCodeRepository
    {
        private readonly INoSQLTableStorage<SmsVerificationCodeEntity> _tableStorage;
        private readonly INoSQLTableStorage<SmsVerificationPriorityCodeEntity> _manualCodesStorage;

        public SmsVerificationCodeRepository(INoSQLTableStorage<SmsVerificationCodeEntity> tableStorage,
            INoSQLTableStorage<SmsVerificationPriorityCodeEntity> manualCodesStorage)
        {
            _tableStorage = tableStorage;
            _manualCodesStorage = manualCodesStorage;
        }

        public async Task<ISmsVerificationCode> CreateAsync(string partnerId, string phoneNum, bool generateRealCode)
        {
            var entity = SmsVerificationCodeEntity.Create(partnerId, phoneNum, DateTime.UtcNow, generateRealCode);
            await _tableStorage.InsertAsync(entity);
            return entity;
        }

        public async Task<ISmsVerificationCode> CreatePriorityAsync(string partnerId, string phoneNum, DateTime expirationDt)
        {
            var entity = SmsVerificationPriorityCodeEntity.Create(partnerId, phoneNum, DateTime.UtcNow, expirationDt);
            await _manualCodesStorage.InsertAsync(entity);
            return entity;
        }

        public async Task<ISmsVerificationCode> GetActualCode(string partnerId, string phoneNum)
        {
            var manualCode =
                await _manualCodesStorage.GetTopRecordAsync(
                    SmsVerificationCodeEntity.GeneratePartitionKey(partnerId, phoneNum));

            if (manualCode == null || manualCode.ExpirationDate < DateTime.UtcNow)
            {
                return await _tableStorage.GetTopRecordAsync(SmsVerificationCodeEntity.GeneratePartitionKey(partnerId, phoneNum));
            }

            return manualCode;
        }

        public async Task<bool> CheckAsync(string partnerId, string phoneNum, string codeToCheck)
        {
            var actualCode = await GetActualCode(partnerId, phoneNum);
            return actualCode != null && actualCode.Code == codeToCheck;
        }

        public async Task DeleteCodesByPhoneNumAsync(string partnerId, string phoneNum)
        {
            await Task.WhenAll(
                DeleteCodesInRepository(partnerId, phoneNum, _tableStorage),
                DeleteCodesInRepository(partnerId, phoneNum, _manualCodesStorage)
            );
        }

        private static async Task DeleteCodesInRepository<T>(string partnerId, string phoneNum, INoSQLTableStorage<T> storage) where T : ITableEntity, new()
        {
            var batchOperation = new TableBatchOperation();
            var entitiesToDelete = (await storage.GetDataAsync(SmsVerificationCodeEntity.GeneratePartitionKey(partnerId, phoneNum))).ToArray();
            if (entitiesToDelete.Any())
            {
                foreach (var e in entitiesToDelete)
                    batchOperation.Delete(e);
                await storage.DoBatchAsync(batchOperation);
            }
        }
    }
}
