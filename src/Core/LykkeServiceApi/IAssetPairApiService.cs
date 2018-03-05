using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;

namespace Core.LykkeServiceApi
{
    public interface IAssetPairApiService
    {
        Task<IEnumerable<AssetPair>> GetMarginalAssetPairsAsync();
    }
}
