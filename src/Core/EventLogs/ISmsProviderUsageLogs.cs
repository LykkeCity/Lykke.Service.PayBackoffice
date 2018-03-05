using System;
using System.Threading.Tasks;

namespace Core.EventLogs
{
    public interface ISmsProviderUsageRecord
    {
        string PhoneNumber { get; }
        string Country { get; }
        string Provider { get; }
        DateTime DateTime { get; }    
    }

    public class SmsProviderUsageRecord : ISmsProviderUsageRecord
    {
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string Provider { get; set; }
        public DateTime DateTime { get; set; }
    }

    public interface ISmsProviderUsageLogs
    {
        Task InsertRecord(ISmsProviderUsageRecord record);
    }
}
