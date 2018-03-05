using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Strategy;
using Flurl;
using Flurl.Http;

namespace LkeServices.Strategy
{
    public class StrategiesSettings
    {
        public string BaseUri { get; set; }
    }

    public class MmApiClient : IMmApiClient
    {
        private readonly StrategiesSettings _settings;

        public MmApiClient(StrategiesSettings settings)
        {
            _settings = settings;
        }

        public async Task<IEnumerable<ApiStrategyRecord>> GetStrategies()
        {
            return await _settings.BaseUri.AppendPathSegment("/strategies")
                .GetJsonAsync<ApiStrategyRecord[]>();
        }

        public async Task<ApiAveragePriceMovementRecord> GetAveragePriceMovement(string assetPairId)
        {
            return await _settings.BaseUri.AppendPathSegment($"/strategies/{nameof(AveragePriceMovement)}/{assetPairId}")
                .GetJsonAsync<ApiAveragePriceMovementRecord>();
        }

        public async Task<ApiMarkUpRecord> GetMarkUp(string assetPairId)
        {
            return await _settings.BaseUri.AppendPathSegment($"/strategies/{nameof(MarkUp)}/{assetPairId}")
                .GetJsonAsync<ApiMarkUpRecord>();
        }

        public async Task EditAveragePriceMovement(string assetPairId, ApiAveragePriceMovementRecord averagePriceMovement)
        {
            await _settings.BaseUri.AppendPathSegment($"/strategies/{nameof(AveragePriceMovement)}/{assetPairId}/update")
                .PostJsonAsync(averagePriceMovement);
        }

        public async Task EditMarkUp(string assetPairId, ApiMarkUpRecord markUp)
        {
            await _settings.BaseUri.AppendPathSegment($"/strategies/{nameof(MarkUp)}/{assetPairId}/update")
                .PostJsonAsync(markUp);
        }
    }
}
