using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class AddressBookEntity : TableEntity, IAddressBookEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsLykkeCorp { get; set; }

        public static AddressBookEntity Create(IAddressBookEntity entity, string partitionKey = null, string rowKey = null)
        {
            var result = new AddressBookEntity();
            result.UpdateFrom(entity);
            result.PartitionKey = partitionKey;
            result.RowKey = rowKey;
            return result;
        }

        public void UpdateFrom(IAddressBookEntity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            IsLykkeCorp = entity.IsLykkeCorp;
        }
    }

    public class AddressBookEntityRepository : IAddressBookEntityRepository
    {
        private const string Partition = "AddressBookEntity";
        private readonly INoSQLTableStorage<AddressBookEntity> _tableStorage;

        public AddressBookEntityRepository(INoSQLTableStorage<AddressBookEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task<IEnumerable<IAddressBookEntity>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<IAddressBookEntity>>(_tableStorage[Partition]);
        }

        public Task<IEnumerable<IAddressBookEntity>> GetLykkeAsync()
        {
            return Task.FromResult<IEnumerable<IAddressBookEntity>>(_tableStorage[Partition].Where(x => x.IsLykkeCorp));
        }

        public Task<IEnumerable<IAddressBookEntity>> GetOtherAsync()
        {
            return Task.FromResult<IEnumerable<IAddressBookEntity>>(_tableStorage[Partition].Where(x => !x.IsLykkeCorp));
        }

        public Task<IAddressBookEntity> GetAsync(string addressBookEntityId)
        {
            return Task.FromResult<IAddressBookEntity>(_tableStorage[Partition, addressBookEntityId]);
        }

        public async Task DeleteAsync(string addressBookEntityId)
        {
            await _tableStorage.DeleteIfExistAsync(Partition, addressBookEntityId);
        }

        public Task SaveAsync(IAddressBookEntity addressBookEntity)
        {
            lock (_tableStorage)
            {
                if (string.IsNullOrWhiteSpace(addressBookEntity.Id))
                {
                    string newId;
                    do
                    {
                        newId = Guid.NewGuid().ToString().ToLowerInvariant();
                    } while (null != _tableStorage[Partition, newId]);
                    addressBookEntity.Id = newId;
                }

                _tableStorage.ModifyOrCreateAsync(Partition, addressBookEntity.Id,
                    () => AddressBookEntity.Create(addressBookEntity, Partition, addressBookEntity.Id),
                    existing => { existing.UpdateFrom(addressBookEntity); }).Wait();
            }
            return Task.CompletedTask;
        }
    }
}