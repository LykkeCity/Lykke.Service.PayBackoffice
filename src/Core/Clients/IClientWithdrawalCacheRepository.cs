using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IClientWithdrawalCacheRepository
    {
        Task<IClientWithdrawalItem> GetAsync(string clientId);
        Task<bool> TrySaveAsync(string clientId, IClientWithdrawalItem item);
    }



    public interface IClientWithdrawalItem
    {
        double Amount { get; }
        string Asset { get; }
        string Bic { get; }
        string AccNumber { get; }
        string AccName { get; }
        string BankName { get; }
        string AccHolderAddress { get; }

        string AccHolderCountry { get; }
        string AccHolderZipCode { get; }
        string AccHolderCity { get; }
    }
}
