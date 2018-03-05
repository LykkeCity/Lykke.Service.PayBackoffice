using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.EventLogs
{
    public interface IRequestsLogRepository
    {
        void Write(string clientId, string url, string request, string response, string userAgent);
        Task<IEnumerable<IRequestsLogRecord>> GetRecords(string clientId, DateTime from, DateTime to);
    }
}
