using System.Linq;
using System.Threading.Tasks;
using Common;
using Core;
using Core.Exchange;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.Assets.Client.Models.Extensions;

namespace LkeServices.Assets.AssetGroups
{
    public class SrvAssetsHelper
    {
        private readonly IAssetsService _assetsService;
        private readonly IExchangeSettingsRepository _exchangeSettingsRepository;
        private readonly CachedTradableAssetsDictionary _tradableAssetsDict;
        private readonly CachedAssetsDictionary _cachedAssetsDictionary;
        private readonly CachedDataDictionary<string, AssetPair>  _assetPairsDict;

        public SrvAssetsHelper(
            IExchangeSettingsRepository exchangeSettingsRepository,
            CachedTradableAssetsDictionary tradableAssetsDict,
            CachedDataDictionary<string, AssetPair> assetPairsDict,
            IAssetsService assetsService, 
            CachedAssetsDictionary cachedAssetsDictionary)
        {
            _exchangeSettingsRepository = exchangeSettingsRepository;
            _tradableAssetsDict = tradableAssetsDict;
            _assetPairsDict = assetPairsDict;
            _assetsService = assetsService;
            _cachedAssetsDictionary = cachedAssetsDictionary;
        }

        public async Task<Asset[]> GetAssetsForClient(string clientId, bool isIosDevice, string partnerId = null, bool getOnlyTradable = true)
        {
            var result = getOnlyTradable ? await _tradableAssetsDict.Values() : await _cachedAssetsDictionary.Values();
                
            result = result.Where(x => !x.IsDisabled);

            if (partnerId != null)
            {
                return result.Where(x => x.PartnerIds != null && x.PartnerIds.Contains(partnerId)).ToArray();
            }

            var assetIdsForClient = await _assetsService.ClientGetAssetIdsAsync(clientId, isIosDevice);

            if (assetIdsForClient.Any())
            {
                result = result.Where(x => assetIdsForClient.Contains(x.Id));
            }

            return result.Where(x => !x.NotLykkeAsset).ToArray();
        }
        
        public Asset[] OrderAssets(Asset[] assets)
        { 
            return assets.OrderBy(x => x.DefaultOrder).ThenBy(x => x.DisplayId ?? x.Id).ToArray();
        }

        public async Task<Asset> GetBaseAssetForClient(string clientId, bool isIosDevice, string partnerId)
        {
            var assetsForClient = (await GetAssetsForClient(clientId, isIosDevice, partnerId)).Where(x => x.IsBase);
            var exchangeSettings = await _exchangeSettingsRepository.GetOrDefaultAsync(clientId);

            var baseAsset = exchangeSettings.BaseAsset(isIosDevice);

            if (string.IsNullOrEmpty(baseAsset))
            {
                baseAsset = assetsForClient.GetFirstAssetId();
            }

            return await _tradableAssetsDict.GetItemAsync(baseAsset);
        }

        public async Task<AssetPair[]> GetAssetsPairsForClient(string clientId, bool isIosDevice, string partnerId, bool ignoreBase = false)
        {
            var assetsForClient = await GetAssetsForClient(clientId, isIosDevice, partnerId);
            var result          = (await _assetPairsDict.Values()).Where(x => !x.IsDisabled);

            if (!ignoreBase)
            {
                result = result.WhichHaveAssets((await GetBaseAssetForClient(clientId, isIosDevice, partnerId)).Id);
            }

            return result.WhichConsistsOfAssets(assetsForClient.Select(x => x.Id).ToArray()).ToArray();
        }
    }
}
