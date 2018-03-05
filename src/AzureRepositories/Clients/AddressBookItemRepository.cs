using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class AddressBookItem : TableEntity, IAddressBookItem
    {
        public string Id { get; set; }
        public string BlockchainColoredAddress { get; set; }
        public string AddressBookEntityId { get; set; }
        public string ClientEmail { get; set; }
        public string Description { get; set; }

        public static AddressBookItem Create(IAddressBookItem entity, string partitionKey = null, string rowKey = null)
        {
            var result = new AddressBookItem();
            result.UpdateFrom(entity);
            result.PartitionKey = partitionKey;
            result.RowKey = rowKey;
            return result;
        }

        public void UpdateFrom(IAddressBookItem entity)
        {
            Id = entity.Id;
            BlockchainColoredAddress = entity.BlockchainColoredAddress;
            AddressBookEntityId = entity.AddressBookEntityId;
            ClientEmail = entity.ClientEmail;
            Description = entity.Description;
        }
    }

    public class AddressBookItemRepository : IAddressBookItemRepository
    {
        private const string Partition = "AddressBookItem";
        private readonly INoSQLTableStorage<AddressBookItem> _tableStorage;

        public AddressBookItemRepository(INoSQLTableStorage<AddressBookItem> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task<IEnumerable<IAddressBookItem>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<IAddressBookItem>>(_tableStorage[Partition]);
        }

        public Task<IEnumerable<IAddressBookItem>> GetBelongingToAsync(params IAddressBookEntity[] entities)
        {
            var entityIds = entities.Select(x => x.Id).ToArray();
            return Task.FromResult<IEnumerable<IAddressBookItem>>(_tableStorage[Partition].Where(x => entityIds.Contains(x.AddressBookEntityId)));
        }

        public Task<IAddressBookItem> GetAsync(string addressBookItemId)
        {
            return Task.FromResult<IAddressBookItem>(_tableStorage[Partition, addressBookItemId]);
        }

        public async Task DeleteAsync(string addressBookItemId)
        {
            await _tableStorage.DeleteIfExistAsync(Partition, addressBookItemId);
        }

        public Task SaveAsync(IAddressBookItem addressBookItem)
        {
            lock (_tableStorage)
            {
                if (string.IsNullOrWhiteSpace(addressBookItem.Id))
                {
                    string newId;
                    do
                    {
                        newId = Guid.NewGuid().ToString().ToLowerInvariant();
                    } while (null != _tableStorage[Partition, newId]);
                    addressBookItem.Id = newId;
                }

                _tableStorage.ModifyOrCreateAsync(Partition, addressBookItem.Id,
                    () => AddressBookItem.Create(addressBookItem, Partition, addressBookItem.Id),
                    existing => { existing.UpdateFrom(addressBookItem); }).Wait();
            }
            return Task.CompletedTask;
        }
    }
}
