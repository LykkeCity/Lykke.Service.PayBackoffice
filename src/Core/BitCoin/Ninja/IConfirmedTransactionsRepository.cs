using System.Threading.Tasks;

namespace Core.BitCoin.Ninja
{
    public interface IConfirmedTransactionsRepository
    {
        Task<bool> SaveConfirmedIfNotExist(string hash);
    }
}
