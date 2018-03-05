using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IAddressBookEntity
    {
        string Id { get; set; }
        string Name { get; set; }
        bool IsLykkeCorp { get; set; }
    }

    public interface IAddressBookEntityRepository
    {
        Task<IEnumerable<IAddressBookEntity>> GetAllAsync();
        Task<IEnumerable<IAddressBookEntity>> GetLykkeAsync();
        Task<IEnumerable<IAddressBookEntity>> GetOtherAsync();

        Task<IAddressBookEntity> GetAsync(string addressBookEntityId);

        Task DeleteAsync(string addressBookEntityId);
        Task SaveAsync(IAddressBookEntity addressBookEntity);
    }
}