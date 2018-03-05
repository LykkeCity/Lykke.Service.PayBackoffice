using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common.Cache;
using Core.Assets;

namespace AzureRepositories.Assets
{
    public class AssetPairDetailedRatesRepository : IAssetPairDetailedRatesRepository
    {
        private readonly IBlobStorage _blobStorage;
        private readonly ICacheManager _cacheManager;
        private const string ContainerName = "history";

        public const string AssetPairHistoryPatternKey = "assets.pairshistory-{0}";
        public const int CacheTime = 2;

        public AssetPairDetailedRatesRepository(IBlobStorage blobStorage,
            ICacheManager cacheManager)
        {
            _blobStorage = blobStorage;
            _cacheManager = cacheManager;
        }

        public async Task<PeriodRecord> GetHistory(string assetPairId, string period)
        {
            var result = new PeriodRecord();
            var blobKey = $"{assetPairId}_{period}";

            if (!await _blobStorage.HasBlobAsync(ContainerName, blobKey))
            {
                return null;
            }

            var key = string.Format(AssetPairHistoryPatternKey, blobKey);
            result.Changes = (await _cacheManager.Get(key, CacheTime,
                async () => await ReadValuesFromStream(_blobStorage[ContainerName, blobKey]))).ToList();
            result.FixingTime = await _blobStorage.GetBlobsLastModifiedAsync(ContainerName);

            return result;
        }

        public async Task<AskBidPeriodRecord> GetHistoryAskBid(string assetPairId, string period)
        {
            var result = new AskBidPeriodRecord();
            var blobKey = $"BA_{assetPairId}_{period}";

            if (!await _blobStorage.HasBlobAsync(ContainerName, blobKey))
            {
                return null;
            }

            var key = string.Format(AssetPairHistoryPatternKey, blobKey);

            result.Changes =
                (await
                    _cacheManager.Get(key, CacheTime,
                        async () => await ReadAskBidValuesFromStream(_blobStorage[ContainerName, blobKey]))).ToList();

            result.FixingTime = await _blobStorage.GetBlobsLastModifiedAsync(ContainerName);

            return result;
        }

        #region Tools

        private async Task<IEnumerable<AskBid>> ReadAskBidValuesFromStream(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string all = await reader.ReadToEndAsync();
                var askBidPairs = all.Split(';');

                return askBidPairs.Select(pair => pair.Split(','))
                    .Select(askBid => new AskBid
                    {
                        A = double.Parse(askBid[0], CultureInfo.InvariantCulture),
                        B = double.Parse(askBid[1], CultureInfo.InvariantCulture)
                    }).ToList();
            }
        }

        private async Task<IEnumerable<double>> ReadValuesFromStream(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string all = await reader.ReadToEndAsync();
                var values = all.Split(';');
                return values.Select(s => double.Parse(s, CultureInfo.InvariantCulture));
            }
        }

        #endregion
    }
}