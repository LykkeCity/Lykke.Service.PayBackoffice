using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.CashOperations
{
    public interface ICashoutLogRecord
    {
        DateTime CreationTime { get; }
        string ClientId { get; }
        string RequestId { get; }
        string TemplateId { get; }
        string Comment { get; }
        string Changer { get;  }
        CashoutTemplateType Type { get; }
    }

    public interface ICashoutTemplateLogRepository
    {
        Task AddRecordAsync(ICashoutLogRecord record, CashoutTemplateType type);
        Task<IEnumerable<ICashoutLogRecord>> GetAllRecordsAsync();
        Task<ICashoutLogRecord> GetDeclineReasonAsync(string requestId);
    }
}
