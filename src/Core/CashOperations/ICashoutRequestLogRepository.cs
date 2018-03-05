using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.CashOperations
{
    public interface ICashoutRequestLogItem
    {
        DateTime CreationTime { get; }
        string RequestId { get; }
        string Changer { get; }
        string ClientName { get; }
        string ClientEmail { get; }
        CashOutRequestStatus Status { get; }
        CashOutVolumeSize VolumeSize { get; }
    }

    public interface ICashoutRequestLogRepository
    {
        Task AddRecordAsync(string changer, string requestId, string clientName, string clientEmail, CashOutRequestStatus status, CashOutVolumeSize volumeSize);
        Task<IEnumerable<ICashoutRequestLogItem>> GetRecords(string requestId);
        Task<IEnumerable<ICashoutRequestLogItem>> GetRecords(IEnumerable<string> requestIds);
    }
}
