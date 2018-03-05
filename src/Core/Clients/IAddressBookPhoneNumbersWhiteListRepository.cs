using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IAddressBookPhoneNumbersWhiteListItem
    {
        string PhoneNumber { get; }
    }

    public interface IAddressBookPhoneNumbersWhiteListRepository
    {
        Task<IEnumerable<IAddressBookPhoneNumbersWhiteListItem>> GetAllAsync();

        Task<IAddressBookPhoneNumbersWhiteListItem> GetAsync(string phoneNumber);

        Task DeleteAsync(IAddressBookPhoneNumbersWhiteListItem phoneNumber);
        Task SaveAsync(IAddressBookPhoneNumbersWhiteListItem phoneNumberItem);
    }
}
