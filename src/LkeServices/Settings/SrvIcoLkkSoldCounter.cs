using System.Linq;
using System.Threading.Tasks;
using Common;
using Core;
using Core.Assets;
using Core.Bitcoin;
using Core.EventLogs;
using Core.Settings;
using Lykke.Service.Assets.Client.Models;

namespace LkeServices.Settings
{
    public class SrvIcoLkkSoldCounter
    {
        private readonly ITempDataRepository _tempDataRepository;
        private readonly ISrvBlockchainReader _srvBlockchainReader;
        private readonly CachedTradableAssetsDictionary _tradableAssetsDictionary;
        private readonly ILkkSourceWalletsRepository _lkkSourceWalletsRepository;
        private readonly IAppGlobalSettingsRepositry _appGlobalSettingsRepositry;

        public SrvIcoLkkSoldCounter(ITempDataRepository tempDataRepository, ISrvBlockchainReader srvBlockchainReader,
            CachedTradableAssetsDictionary tradableAssetsDictionary, ILkkSourceWalletsRepository lkkSourceWalletsRepository,
            IAppGlobalSettingsRepositry appGlobalSettingsRepositry)
        {
            _tempDataRepository = tempDataRepository;
            _srvBlockchainReader = srvBlockchainReader;
            _tradableAssetsDictionary = tradableAssetsDictionary;
            _lkkSourceWalletsRepository = lkkSourceWalletsRepository;
            _appGlobalSettingsRepositry = appGlobalSettingsRepositry;
        }

        public async Task<double> GetLkkSoldAmount()
        {
            var lkk = await _tradableAssetsDictionary.GetItemAsync(LykkeConstants.LykkeAssetId);

            var walletsToTrack = await _lkkSourceWalletsRepository.GetRecordsAsync();

            double result = 0;

            var balancesTasks =
                walletsToTrack.Select(
                    x =>
                        _srvBlockchainReader.GetBalanceForAdress(x.Address, lkk)
                            .ContinueWith(task => x.StartBalance - task.Result.Balance));

            var balances = await Task.WhenAll(balancesTasks);

            result += balances.Sum();

            result += (await _appGlobalSettingsRepositry.GetAsync()).IcoLkkSold;

            var maxValue =
                (await _tempDataRepository.RetrieveData<IcoCoinsBoughtData>())?.MaxValue ?? 0;

            if (result > maxValue)
            {
                await _tempDataRepository.InsertOrReplaceDataAsync(new IcoCoinsBoughtData { MaxValue = result });
                maxValue = result;
            }

            return maxValue;
        }
    }
}
