using System.Threading.Tasks;
using Core.Settings;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Core.MarginTrading;

namespace LkeServices.MarginTrading
{
    public class MarginTradingSettingsService : IMarginTradingSettingsService
    {
        private readonly IClientAccountClient _clientAccountService;
        private readonly IAppGlobalSettingsRepositry _appGlobalSettingsRepositry;

        public MarginTradingSettingsService(IClientAccountClient clientAccountService,
            IAppGlobalSettingsRepositry appGlobalSettingsRepositry)
        {
            _clientAccountService = clientAccountService;
            _appGlobalSettingsRepositry = appGlobalSettingsRepositry;
        }

        public async Task<MarginEnabledSettingsModel> GetMargingSettings(string clientId)
        {
            var marginTradingEnabled = (await _appGlobalSettingsRepositry.GetAsync()).MarginTradingEnabled;
            var userMarginTradingSettings = await _clientAccountService.GetMarginEnabledAsync(clientId);

            return new MarginEnabledSettingsModel
            {
                Enabled = marginTradingEnabled && userMarginTradingSettings.Enabled,
                EnabledLive = marginTradingEnabled && userMarginTradingSettings.EnabledLive,
                TermsOfUseAgreed = userMarginTradingSettings.TermsOfUseAgreed
            };
        }

        public async Task SetTermsOfUseAgreed(string clientId)
        {
            var userMarginTradingSettings = await _clientAccountService.GetMarginEnabledAsync(clientId);

            await _clientAccountService.SetMarginEnabledAsync(clientId, userMarginTradingSettings.Enabled,
                userMarginTradingSettings.EnabledLive, true);
        }

        public async Task<bool> IsTermsOfUseAgreed(string clientId)
        {
            var userMarginTradingSettings = await _clientAccountService.GetMarginEnabledAsync(clientId);

            return userMarginTradingSettings.TermsOfUseAgreed;
        }

        public async Task<bool> IsMargingTradingDemoEnabled(string clientId)
        {
            return (await _appGlobalSettingsRepositry.GetAsync()).MarginTradingEnabled &&
                   (await _clientAccountService.GetMarginEnabledAsync(clientId)).Enabled;
        }

        public async Task<bool> IsMargingTradingLiveEnabled(string clientId)
        {
            return (await _appGlobalSettingsRepositry.GetAsync()).MarginTradingEnabled &&
                   (await _clientAccountService.GetMarginEnabledAsync(clientId)).EnabledLive;
        }
    }
}
