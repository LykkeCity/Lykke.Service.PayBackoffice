using System.Threading.Tasks;
using Core.Exchange;

namespace Core.EventLogs
{
    public interface IPurchaseAttemptsLog
    {
        Task RegisterAsync(IMarketOrder marketOrder);
    }

}
