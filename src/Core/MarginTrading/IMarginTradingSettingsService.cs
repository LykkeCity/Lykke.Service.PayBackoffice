using Lykke.Service.ClientAccount.Client.Models;
using System.Threading.Tasks;

namespace Core.MarginTrading
{
    public interface IMarginTradingSettingsService
    {
        Task<bool> IsMargingTradingDemoEnabled(string clientId);
        Task<bool> IsMargingTradingLiveEnabled(string clientId);
        Task<bool> IsTermsOfUseAgreed(string clientId);
        Task<MarginEnabledSettingsModel> GetMargingSettings(string clientId);
        Task SetTermsOfUseAgreed(string clientId);
    }
}
