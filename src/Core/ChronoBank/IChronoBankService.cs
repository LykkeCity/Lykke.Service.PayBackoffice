using System.Threading.Tasks;
using Core.BitCoin;

namespace Core.ChronoBank
{
    public interface IChronoBankService
    {
        Task<string> SetNewChronoBankContract(IWalletCredentials walletCredentials);
        Task SendCashOutRequest(string id, string addressTo, double amount);
    }
}