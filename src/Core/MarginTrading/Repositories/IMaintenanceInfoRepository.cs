using Core.MarginTrading.Models;
using System.Threading.Tasks;

namespace Core.MarginTrading.Repositories
{
    public interface IMaintenanceInfoRepository
    {
        Task<IMaintenanceInfo> GetMaintenanceInfo(bool isDemo);
        Task SetMaintenanceInfo(IMaintenanceInfo record, bool isDemo);
    }
}
