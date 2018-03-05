using System.Threading.Tasks;
using AzureStorage;
using Common.PasswordTools;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{

    public class PinSecurityEntity : TableEntity, IPasswordKeeping
    {
        public static string GeneratePartitionKey()
        {
            return "Pin";
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }


        public string Hash { get; set; }
        public string Salt { get; set; }


        public static PinSecurityEntity Create(string clientId, string pin)
        {
            var result = new PinSecurityEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(clientId)
            };

            result.SetPassword(pin);

            return result;
        }

    }

    public class PinSecurityRepository : IPinSecurityRepository
    {
        private readonly INoSQLTableStorage<PinSecurityEntity> _tableStorage;

        public PinSecurityRepository(INoSQLTableStorage<PinSecurityEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task SaveAsync(string clientId, string pin)
        {
            var entity = PinSecurityEntity.Create(clientId, pin);
            return _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task<bool> CheckAsync(string clientId, string pin)
        {
            var partitionKey = PinSecurityEntity.GeneratePartitionKey();
            var rowKey = PinSecurityEntity.GenerateRowKey(clientId);
            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);
            return entity != null && entity.CheckPassword(pin);
        }

        public async Task<bool> IsPinEntered(string clientId)
        {
            var partitionKey = PinSecurityEntity.GeneratePartitionKey();
            var rowKey = PinSecurityEntity.GenerateRowKey(clientId);
            return await _tableStorage.GetDataAsync(partitionKey, rowKey) != null;
        }

        //Pin was moved to client side and it is needed to retrieve it from hash
        public async Task<string> GetPin(string clientId)
        {
            var partitionKey = PinSecurityEntity.GeneratePartitionKey();
            var rowKey = PinSecurityEntity.GenerateRowKey(clientId);
            var entity = await _tableStorage.GetDataAsync(partitionKey, rowKey);

            if (entity != null)
            {
                for (int i = 0; i < 10000; ++i)
                {
                    var possiblePin = i.ToString("0000");
                    if (entity.CheckPassword(possiblePin))
                        return possiblePin;
                }
            }

            return null;
        }
    }
}
