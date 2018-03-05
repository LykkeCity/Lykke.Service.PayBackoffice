using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Accounts
{
    public interface IBalancePendingRecord
    {
        string Id { get; }
        string ClientId { get; }
        string AssetId { get; }
        double BalancePending { get; }
        string BlockChainHash { get; }
    }

    public interface IBalancePendingRepository
    {
        Task<IEnumerable<IBalancePendingRecord>> GetAsync(string clientId);
        Task<string> CreateAsync(string clientId, string assetId, double balance);
        Task UpdateBlockchainHashAsync(string clientId, string recordId, string hash);
        Task RemoveByBcnHash(string clientId, string hash);
    }
}
