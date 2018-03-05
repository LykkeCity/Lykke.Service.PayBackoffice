using Core.AddressBook;
using Core.Clients;
using System.Threading.Tasks;

namespace LkeServices.AddressBook
{
    public class AddressBookService : IAddressBookService
    {
        private readonly IAddressBookPhoneNumbersWhiteListRepository _phoneNumbersWhiteListRepository;

        public AddressBookService(
            IAddressBookPhoneNumbersWhiteListRepository phoneNumbersWhiteListRepository
            )
        {
            _phoneNumbersWhiteListRepository = phoneNumbersWhiteListRepository;
        }

        public async Task<bool> isPhoneNumberInWhiteList(string phoneNumber)
        {
            return (await _phoneNumbersWhiteListRepository.GetAsync(phoneNumber)) != null;
        }
    }
}
