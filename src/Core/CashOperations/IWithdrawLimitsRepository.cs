using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.CashOperations
{
    public interface IWithdrawLimit
    {
        string  AssetId { get; }
        double LimitAmount { get;  }
    }

    public interface IWithdrawLimitsRepository
    {
        Task<IEnumerable<IWithdrawLimit>> GetDataAsync();
        Task<bool> AddAsync(IWithdrawLimit item);
        Task<bool> DeleteAsync(string assetId);
        Task<double> GetLimitByAssetAsync(string assetId);
        double DefaultWithdrawalLimit { get;  }
    }
}
