using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class AddressBookPhoneNumbersWhiteListEntity : TableEntity, IAddressBookPhoneNumbersWhiteListItem
    {
        public string PhoneNumber { get => RowKey; set => RowKey = value; }
    }

    public class AddressBookPhoneNumbersWhiteListRepository : IAddressBookPhoneNumbersWhiteListRepository
    {
        private const string Partition = "AddressBookPhoneNumbersWhiteList";
        private readonly INoSQLTableStorage<AddressBookPhoneNumbersWhiteListEntity> _tableStorage;

        public AddressBookPhoneNumbersWhiteListRepository(INoSQLTableStorage<AddressBookPhoneNumbersWhiteListEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IAddressBookPhoneNumbersWhiteListItem>> GetAllAsync()
        {
            return await _tableStorage.GetDataAsync(Partition);
        }

        public async Task<IAddressBookPhoneNumbersWhiteListItem> GetAsync(string phoneNumber)
        {
            return await _tableStorage.GetDataAsync(Partition, phoneNumber);
        }

        public async Task DeleteAsync(IAddressBookPhoneNumbersWhiteListItem phoneNumberItem)
        {
            await _tableStorage.DeleteAsync(Partition, phoneNumberItem.PhoneNumber);
        }

        public async Task SaveAsync(IAddressBookPhoneNumbersWhiteListItem phoneNumberItem)
        {
            AddressBookPhoneNumbersWhiteListEntity entity = new AddressBookPhoneNumbersWhiteListEntity();
            entity.PartitionKey = Partition;
            entity.RowKey = phoneNumberItem.PhoneNumber;
            await _tableStorage.InsertOrReplaceAsync(entity);
        }

    }
}
