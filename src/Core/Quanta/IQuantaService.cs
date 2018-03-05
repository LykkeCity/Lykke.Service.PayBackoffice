using System.Threading.Tasks;
using Core.BitCoin;

namespace Core.Quanta
{
    public interface IQuantaService
    {
        Task<string> SetNewQuantaContract(IWalletCredentials walletCredentials);
        Task SendCashOutRequest(string id, string addressTo, double amount);
        Task<bool> IsQuantaUser(string address);
    }
}