using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.AddressBook
{
    public interface IAddressBookService
    {
        Task<bool> isPhoneNumberInWhiteList(string phoneNumber);
    }
}
