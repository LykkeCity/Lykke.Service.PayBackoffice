using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.PaymentSystems;

namespace Core.Clients
{
    public enum RequestState
    {
        Unknown,
        New,
        Processed,
        Skipped
    }

    public interface IRequestVoiceCallRecord
    {
        string ClientId { get; }
        RequestState RequestState { get; }
        DateTime DateTime { get; }
        string PhoneNumber { get; }
        string ProcessedBy { get; }
    }

    public interface IRequestVoiceCallRepository
    {
        Task InsertRequestAsync(string clientId, string phoneNumber);
        Task HandleRequestProcessedAsync(string clientId, string processedBy);
        Task HandleSkipRequestAsync(string clientId, string processedBy);

        Task<IEnumerable<IRequestVoiceCallRecord>> GetNewAsync();

        Task<IEnumerable<IRequestVoiceCallRecord>> GetHistoryRecordsAsync(DateTime from, DateTime to,
            Func<IRequestVoiceCallRecord, bool> filter = null);
    }
}
