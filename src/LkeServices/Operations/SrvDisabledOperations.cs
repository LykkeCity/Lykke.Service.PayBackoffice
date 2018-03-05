using System;
using System.Threading.Tasks;
using Common;
using Core.Assets;
using Core.EventLogs;
using Core.Settings;
using System.Linq;
using Core;
using Lykke.Service.Assets.Client.Models;
using Blockchain = Lykke.Service.Assets.Client.Models.Blockchain;

namespace LkeServices.Operations
{
    public class SrvDisabledOperations
    {
        private readonly CachedDataDictionary<string, AssetPair> _assetsPairsDict;
        private readonly CachedTradableAssetsDictionary _tradableAssetsDict;
        private readonly IAppGlobalSettingsRepositry _appGlobalSettingsRepo;

        public SrvDisabledOperations(
            CachedDataDictionary<string, AssetPair> assetsPairsDict,
            CachedTradableAssetsDictionary tradableAssetsDict,
            IAppGlobalSettingsRepositry appGlobalSettingsRepo)
        {
            _assetsPairsDict = assetsPairsDict;
            _tradableAssetsDict = tradableAssetsDict;
            _appGlobalSettingsRepo = appGlobalSettingsRepo;
        }

        public async Task<bool> IsOperationForAssetDisabled(string assetId)
        {
            var settings = await _appGlobalSettingsRepo.GetAsync();
            bool btcBlockchainOpsDisabled = settings.BitcoinBlockchainOperationsDisabled;
            bool btcOnlyDisabled = settings.BtcOperationsDisabled;

            return btcOnlyDisabled && assetId == LykkeConstants.BitcoinAssetId ||
                btcBlockchainOpsDisabled && await IsBtcBlockchainAsset(assetId);
        }

        public async Task<bool> IsOperationForAssetPairDisabled(string assetPairId)
        {
            var pair = await _assetsPairsDict.GetItemAsync(assetPairId);
            return await IsOperationForAssetDisabled(pair.BaseAssetId) ||
                   await IsOperationForAssetDisabled(pair.QuotingAssetId);
        }

        private async Task<bool> IsBtcBlockchainAsset(string assetId)
        {
            var asset = await _tradableAssetsDict.GetItemAsync(assetId);
            return asset.Blockchain == Blockchain.Bitcoin;
        }
    }
}
