using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IClientBalanceChangeLogRecord
    {
        string ClientId { get; }
        DateTime TransactionTimestamp { get; }
        string TransactionId { get; }
        string TransactionType { get; }
        string Asset { get; }
        double OldBalance { get; }
        double NewBalance { get; }
    }

    public interface IClientBalanceChangeLogRepository
    {
        Task<IEnumerable<IClientBalanceChangeLogRecord>> GetByClientAsync(string clientId);
    }
}
