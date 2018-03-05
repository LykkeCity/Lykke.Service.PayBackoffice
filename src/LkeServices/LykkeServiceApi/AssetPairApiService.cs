using System.Collections.Generic;
using System.Threading.Tasks;
using Core.LykkeServiceApi;
using Lykke.Service.Assets.Client.Models;

namespace LkeServices.LykkeServiceApi
{
    public class AssetPairApiService : IAssetPairApiService
    {
        private readonly ILykkeServiceApiConnector _apiConnector;

        public AssetPairApiService(ILykkeServiceApiConnector apiConnector)
        {
            _apiConnector = apiConnector;
        }

        public async Task<IEnumerable<AssetPair>> GetMarginalAssetPairsAsync()
        {
            var requestUrl = "marginalassetpair";

            return await _apiConnector.GetAsync<AssetPair>(requestUrl);
        }
    }
}
