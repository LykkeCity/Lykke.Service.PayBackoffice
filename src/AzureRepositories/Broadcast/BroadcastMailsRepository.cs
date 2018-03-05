using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Broadcast;
using Lykke.Messages.Email;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Broadcast
{
    public class BroadcastMailEntity : TableEntity, IBroadcastMail
    {
        public string Email => RowKey;

        public static string GeneratePartitionKey(BroadcastGroup broadcastGroup)
        {
            return ((int)broadcastGroup).ToString();
        }

        public static string GenerateRowKey(string email)
        {
            return email;
        }

        public BroadcastGroup Group
        {
            get { return (BroadcastGroup)int.Parse(PartitionKey); }
            set
            {
                PartitionKey = ((int)value).ToString();
            }
        }

        public static BroadcastMailEntity Create(IBroadcastMail broadcastMail)
        {
            return new BroadcastMailEntity
            {
                RowKey = broadcastMail.Email,
                Group = broadcastMail.Group
            };
        }
    }

    public class BroadcastMailsRepository : IBroadcastMailsRepository
    {
        private readonly INoSQLTableStorage<BroadcastMailEntity> _tableStorage;

        public BroadcastMailsRepository(INoSQLTableStorage<BroadcastMailEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task SaveAsync(IBroadcastMail broadcastMail)
        {
            var entity = BroadcastMailEntity.Create(broadcastMail);
            await _tableStorage.InsertAsync(entity);
        }

        public async Task<IEnumerable<IBroadcastMail>> GetEmailsByGroup(BroadcastGroup group)
        {
            return await _tableStorage.GetDataAsync(BroadcastMailEntity.GeneratePartitionKey(group));
        }

        public async Task DeleteAsync(IBroadcastMail broadcastMail)
        {
            await _tableStorage.DeleteAsync(BroadcastMailEntity.GeneratePartitionKey(broadcastMail.Group),
                BroadcastMailEntity.GenerateRowKey(broadcastMail.Email));
        }

	    public async Task DeleteAsync(BroadcastGroup @group, string email)
	    {
			await _tableStorage.DeleteAsync(BroadcastMailEntity.GeneratePartitionKey(@group),
			  BroadcastMailEntity.GenerateRowKey(email));
		}

	    public bool RecordAlreadyExists(IBroadcastMail broadcastMail)
        {
            var entity = BroadcastMailEntity.Create(broadcastMail);
            return _tableStorage.RecordExists(entity);
        }
    }
}
