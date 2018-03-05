using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IClientBalancesService
    {
        Task<ClientBalance> GetTotalTradingBalance(string clientId, string baseAssetId);
        Task<double> GetTotalPrivateWalletsBalance(string clientId, string baseAssetId);
    }
}
