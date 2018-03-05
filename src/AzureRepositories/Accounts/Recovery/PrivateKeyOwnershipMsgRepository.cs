using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.Accounts.Recovery;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Accounts.Recovery
{
    public class PrivateKeyOwnershipMsgEntity : TableEntity
    {
        public string Email => PartitionKey;
        public string Message { get; set; }

        public static string GeneratePartitionKey()
        {
            return "OwnershipMsg";
        }

        public static string GenerateRowKey(string partnerId, string email)
        {
            if(string.IsNullOrWhiteSpace(partnerId))
                return email;
            return $"{email}_{partnerId}";
        }

        public static PrivateKeyOwnershipMsgEntity Create(string partnerId, string email, string message)
        {
            return new PrivateKeyOwnershipMsgEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(partnerId, email),
                Message = message
            };
        }
    }

    public class PrivateKeyOwnershipMsgRepository : IPrivateKeyOwnershipMsgRepository
    {
        private readonly INoSQLTableStorage<PrivateKeyOwnershipMsgEntity> _tableStorage;

        public PrivateKeyOwnershipMsgRepository(INoSQLTableStorage<PrivateKeyOwnershipMsgEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<string> GenerateMsgForEmail(string partnerId, string email)
        {
            var msg = Guid.NewGuid().ToString("N");
            var entity = PrivateKeyOwnershipMsgEntity.Create(partnerId, email, msg);
            await _tableStorage.InsertOrReplaceAsync(entity);
            return msg;
        }

        public async Task<string> GetMsgForEmail(string partnerId, string email)
        {
            return (await _tableStorage.GetDataAsync(PrivateKeyOwnershipMsgEntity.GeneratePartitionKey(),
                PrivateKeyOwnershipMsgEntity.GenerateRowKey(partnerId, email))).Message;
        }

        public Task RemoveMsg(string partnerId, string email)
        {
            return _tableStorage.DeleteAsync(PrivateKeyOwnershipMsgEntity.GeneratePartitionKey(),
                PrivateKeyOwnershipMsgEntity.GenerateRowKey(partnerId, email));
        }
    }
}
