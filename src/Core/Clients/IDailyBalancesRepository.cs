using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IDailyBalanceRecord
    {
        string AssetId { get; set; }
        double Amount { get; set; }
    }

    public class DailyBalanceRecord : IDailyBalanceRecord
    {
        public string AssetId { get; set; }
        public double Amount { get; set; }
    }

    public interface IDailyBalancesRepository
    {
        Task<IEnumerable<DailyBalanceRecord>> GetDailyBalance(string clientId, DateTime day);
        Task InsertDailyBalance(string clientId, DateTime day, IEnumerable<DailyBalanceRecord> balances);
    }
}
