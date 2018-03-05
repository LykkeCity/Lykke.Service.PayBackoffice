using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface IWalletsGenerationHistory
    {
        Task AddWalletGenerationRecord(string clientId, string multiSig);
        Task AddWalletDisactivatedRecord(string clientId, string multiSig);
    }
}
