using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Assets;
using Lykke.Service.Assets.Client.Models;

namespace Core.LykkeServiceApi
{
    public interface IAssetApiService
    {
        Task<IEnumerable<Asset>> GetMarginalAssetsAsync();
    }
}
