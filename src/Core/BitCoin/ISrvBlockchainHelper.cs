using System.Threading.Tasks;

namespace Core.BitCoin
{
    public interface ITransaction
    {
        /// <summary>
        /// Internal id for generated transaction
        /// </summary>
        string Id { get; }

        string Hex { get; }
    }

    public class TransactionDto : ITransaction
    {
        public string Id { get; set; }
        public string Hex { get; set; }
    }


    public interface ISrvBlockchainHelper
    {
        Task<IWalletCredentials> GenerateWallets(string clientId, string clientPubKeyHex, string encodedPrivateKey, /*todo: remove*/string privateKey, NetworkType networkType);

        Task<ITransaction> GenerateTransferTransaction(string sourceAddress, string destAddress, double amount, string bcnAssetId);

        Task BroadcastTransaction(ITransaction transaction);

        bool VerifyMessage(string pubKeyAddress, string message, string signedMessage);
    }
}
