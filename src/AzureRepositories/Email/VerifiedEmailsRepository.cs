using System.Threading.Tasks;
using AzureStorage;
using Core.Messages.Email;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Email
{
    public class VerifiedEmailEntity : TableEntity
    {
        public static string GeneratePartion(string partnerId)
        {
            string partition = string.IsNullOrEmpty(partnerId) ? "VerifiedEmail" : $"VerifiedEmail_{partnerId}";
            return partition;
        }

        public static string GenerateRowKey(string email)
        {
            return email;
        }

        public static VerifiedEmailEntity Create(string email, string partnerId)
        {
            return new VerifiedEmailEntity()
            {
                PartitionKey = GeneratePartion(partnerId),
                RowKey = GenerateRowKey(email)
            };
        }
    }

    public class VerifiedEmailsRepository : IVerifiedEmailsRepository
    {
        private readonly INoSQLTableStorage<VerifiedEmailEntity> _tableStorage;

        public VerifiedEmailsRepository(INoSQLTableStorage<VerifiedEmailEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task AddOrReplaceAsync(string email, string partnerId)
        {
            var entity = VerifiedEmailEntity.Create(email, partnerId);
            return _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task<bool> IsEmailVerified(string email, string partnerId)
        {
            var entity = await _tableStorage.GetDataAsync(VerifiedEmailEntity.GeneratePartion(partnerId),
                VerifiedEmailEntity.GenerateRowKey(email));
            return entity != null;
        }

        public async Task RemoveAsync(string email, string partnerId)
        {
            await _tableStorage.DeleteAsync(
                VerifiedEmailEntity.GeneratePartion(partnerId),
                VerifiedEmailEntity.GenerateRowKey(email));
        }

        public async Task ChangeEmailAsync(string email, string partnerId, string newEmail)
        {
            var deleted = await _tableStorage.DeleteIfExistAsync(
                VerifiedEmailEntity.GeneratePartion(partnerId),
                VerifiedEmailEntity.GenerateRowKey(email));

            if (deleted)
            {
                await _tableStorage.InsertAsync(VerifiedEmailEntity.Create(newEmail, partnerId));
            }
        }
    }
}
