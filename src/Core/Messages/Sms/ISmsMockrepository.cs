using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Messages.Sms
{
    public interface ISmsMockRecord
    {
        string Id { get; }
        string PhoneNumber { get; }
        DateTime DateTime { get; }
        string From { get; }
        string Text { get; }
    }

    public interface ISmsMockRepository
    {
        Task InsertAsync(string phoneNumber, SmsMessage msg);

        Task<IEnumerable<ISmsMockRecord>> GetAllAsync();

        Task<IEnumerable<ISmsMockRecord>> Get(string email);

        Task<ISmsMockRecord> GetAsync(string email, string id);
    }
}
