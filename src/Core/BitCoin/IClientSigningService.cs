using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface IClientSigningService
    {
        Task PushClientKey(string privateKey);
        Task<string> SignTransaction(string transactionHex);
        Task<string> GetPrivateKey(string address);
    }
}
