using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IAddressBookItem
    {
        string Id { get; set; }
        string BlockchainColoredAddress { get; set; }
        string ClientEmail { get; set; }
        string AddressBookEntityId { get; set; }
        string Description { get; set; }
    }

    public interface IAddressBookItemRepository
    {
        Task<IEnumerable<IAddressBookItem>> GetAllAsync();
        Task<IEnumerable<IAddressBookItem>> GetBelongingToAsync(params IAddressBookEntity[] entities);

        Task<IAddressBookItem> GetAsync(string addressBookItemId);

        Task DeleteAsync(string addressBookItemId);
        Task SaveAsync(IAddressBookItem addressBookItem);
    }
}