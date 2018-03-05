using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class RecoveryStatusEntity : TableEntity
    {
        public const string Partition = "RecoveryStatus";

        public RecoveryStatusEntity() { }
        public RecoveryStatusEntity(string clientId, RecoveryStatus status)
        {
            PartitionKey = Partition;
            RowKey = clientId;
            Status = (int)status;
            DateTime = DateTime.Now;
        }

        public int Status { get; set; }
        public DateTime DateTime { get; set; }

        public RecoveryStatus RecoveryStatus => (RecoveryStatus) Status;
    }

    public class RecoveryTokenChallangeEntity : TableEntity
    {
        public const string Partition = "TokenChallange";

        public RecoveryTokenChallangeEntity() { }
        public RecoveryTokenChallangeEntity(string clientId, string message)
        {
            PartitionKey = Partition;
            RowKey = clientId;
            Message = message;
        }

        public string Message { get; set; }
    }

    public class RecoveryTokenLevel1Entity : TableEntity, IAccessTokenLvl1
    {
        public const string Partition = "TokenLevel1";

        public RecoveryTokenLevel1Entity() { }
        public RecoveryTokenLevel1Entity(string clientId, string accessToken)
        {
            IssueDateTime = DateTime.Now;
            PartitionKey = Partition;
            RowKey = accessToken;
            ClientId = clientId;
        }

        public string ClientId { get; set; }
        public string AccessToken => RowKey;
        public DateTime IssueDateTime { get; set; }

        public string EmailVerificationCode { get; set; }
        public string PhoneVerificationCode { get; set; }
        public DateTime? VerificationSendDateTime { get; set; }
    }


    public class RecoveryTokensRepository : IRecoveryTokensRepository
    {
        private readonly INoSQLTableStorage<RecoveryStatusEntity> _recoveryStatusEntities;
        private readonly INoSQLTableStorage<RecoveryTokenChallangeEntity> _recoveryTokenChallangeEntities;
        private readonly INoSQLTableStorage<RecoveryTokenLevel1Entity> _recoveryTokenLevel1Entities;

        public RecoveryTokensRepository(INoSQLTableStorage<RecoveryStatusEntity> recoveryStatusEntities,
            INoSQLTableStorage<RecoveryTokenChallangeEntity> recoveryTokenChallangeEntities,
            INoSQLTableStorage<RecoveryTokenLevel1Entity> recoveryTokenLevel1Entities)
        {
            _recoveryStatusEntities = recoveryStatusEntities;
            _recoveryTokenChallangeEntities = recoveryTokenChallangeEntities;
            _recoveryTokenLevel1Entities = recoveryTokenLevel1Entities;
        }

        public Task SetRecoveryStatusAsync(string clientId, RecoveryStatus status)
        {
            var entity = new RecoveryStatusEntity(clientId, status);
            return status != RecoveryStatus.None
                ? _recoveryStatusEntities.InsertOrReplaceAsync(entity)
                : _recoveryStatusEntities.DeleteIfExistAsync(entity.PartitionKey, entity.RowKey);
        }

        public async Task<Tuple<DateTime, RecoveryStatus>> GetRecoveryStatusAsync(string clientId)
        {
            var entity = await _recoveryStatusEntities
                .GetDataAsync(RecoveryStatusEntity.Partition, clientId);

            return new Tuple<DateTime, RecoveryStatus>(
                entity?.DateTime ?? DateTime.Now,
                entity?.RecoveryStatus ?? RecoveryStatus.None);
        }

        public async Task<IEnumerable<Tuple<string, DateTime, RecoveryStatus>>> GetAllRecoveryStatusesAsync()
        {
            return (await _recoveryStatusEntities
                .GetDataAsync(RecoveryStatusEntity.Partition))
                .Select(x => new Tuple<string, DateTime, RecoveryStatus>(
                    x.RowKey, x.DateTime, x.RecoveryStatus));
        }

        public async Task RegisterChallangeAsync(string clientId, string message)
        {
            var entity = new RecoveryTokenChallangeEntity(clientId, message);
            await _recoveryTokenChallangeEntities.InsertOrReplaceAsync(entity);
        }

        public async Task<string> GetChallangeAsync(string clientId)
        {
            var entity = await _recoveryTokenChallangeEntities
                .GetDataAsync(RecoveryTokenChallangeEntity.Partition, clientId);
            return entity?.Message;
        }

        public async Task RemoveChallangeAsync(string clientId)
        {
            await _recoveryTokenChallangeEntities
                .DeleteIfExistAsync(RecoveryTokenChallangeEntity.Partition, clientId);
        }

        public async Task RegisterAccessTokenLvl1Async(string clientId, string accessTokenLvl1)
        {
            var entity = new RecoveryTokenLevel1Entity(clientId, accessTokenLvl1);
            await _recoveryTokenLevel1Entities.InsertAsync(entity);
        }

        public async Task<IAccessTokenLvl1> GetAccessTokenLvl1Async(string accessTokenLvl1)
        {
            var entity = await _recoveryTokenLevel1Entities
                .GetDataAsync(RecoveryTokenLevel1Entity.Partition, accessTokenLvl1);
            return entity;
        }

        public async Task RemoveAccessTokenLvl1Async(string accessTokenLvl1)
        {
            await _recoveryTokenLevel1Entities
                .DeleteIfExistAsync(RecoveryTokenLevel1Entity.Partition, accessTokenLvl1);
        }

        public async Task SetVerificationCodes(string accessTokenLvl1,
            string emailVerificationCode, string phoneVerificationCode)
        {
            var entity = await _recoveryTokenLevel1Entities
                .GetDataAsync(RecoveryTokenLevel1Entity.Partition, accessTokenLvl1);
            
            if (entity == null)
                return;

            await _recoveryTokenLevel1Entities.ReplaceAsync(entity, e =>
            {
                e.EmailVerificationCode = emailVerificationCode;
                e.PhoneVerificationCode = phoneVerificationCode;
                e.VerificationSendDateTime = DateTime.Now;
                return e;
            });
        }
    }
}
