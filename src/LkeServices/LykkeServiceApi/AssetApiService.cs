using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Assets;
using Core.LykkeServiceApi;
using Lykke.Service.Assets.Client.Models;

namespace LkeServices.LykkeServiceApi
{
    public class AssetApiService : IAssetApiService
    {
        private readonly ILykkeServiceApiConnector _apiConnector;

        public AssetApiService(ILykkeServiceApiConnector apiConnector)
        {
            _apiConnector = apiConnector;
        }

        public async Task<IEnumerable<Asset>> GetMarginalAssetsAsync()
        {
            var requestUrl = "marginalasset";

            return await _apiConnector.GetAsync<Asset>(requestUrl);
        }
    }
}
