using System;
using System.Threading.Tasks;
using Core.EventLogs;
using Core.Settings;
using System.Linq;
using Core;

namespace LkeServices.Operations
{
    public class SrvCashOutRules
    {
        private readonly ICallTimeLimitsRepository _callTimeLimitsRepository;
        private readonly CachedTradableAssetsDictionary _tradableAssetsDict;
        private readonly IAppGlobalSettingsRepositry _appGlobalSettingsRepo;
        private const string MethodName = "CashOutOperation";

        public SrvCashOutRules(
            ICallTimeLimitsRepository callTimeLimitsRepository,
            CachedTradableAssetsDictionary tradableAssetsDict,
            IAppGlobalSettingsRepositry appGlobalSettingsRepo)
        {
            _callTimeLimitsRepository = callTimeLimitsRepository;
            _tradableAssetsDict = tradableAssetsDict;
            _appGlobalSettingsRepo = appGlobalSettingsRepo;
        }

        public async Task<bool> IsCashOutEnabled(string clientId, string assetId, double amount)
        {
            var asset = await _tradableAssetsDict.GetItemAsync(assetId);

            //cash outs has no limits when volume is high enough
            if (amount >= asset.LowVolumeAmount)
                return true;

            var settings = await _appGlobalSettingsRepo.GetAsync();

            var timeout = TimeSpan.FromMinutes(settings.LowCashOutTimeoutMins);
            var callHistory =
                await
                    _callTimeLimitsRepository.GetCallHistoryAsync(MethodName, clientId,
                        timeout);

            return !callHistory.Any() || callHistory.IsCallEnabled(timeout, settings.LowCashOutLimit);
        }

        public async Task NoteSuccessfulCashOut(string clientId, string assetId, double amount)
        {
            var asset = await _tradableAssetsDict.GetItemAsync(assetId);

            if (amount < asset.LowVolumeAmount)
                await _callTimeLimitsRepository.InsertRecordAsync(MethodName, clientId);
        }
    }
}
