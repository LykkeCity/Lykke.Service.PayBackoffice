using System.Threading.Tasks;

namespace Core.Exchange
{
    public interface ILimitOrderService
    {
        Task CreateAsync(string pushTemplate, string id, string clientId, string assetPairId, double volume, double price,
            double remainigVolume);

        Task CancelAsync(string pushTemplate, string id);

        Task RejectAsync(string pushTemplate, string id, string clientId, string assetPairId, double volume, double price, string status);

        Task RemoveAsync(string id, string clientId);
    }
}
