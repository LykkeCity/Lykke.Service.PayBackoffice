using System.IO;
using System.Threading.Tasks;

namespace Core.Accounts.PrivateWallets
{
    public interface IBackupQrRepository
    {
        /// <summary>
        /// Saves QR and returns url to file
        /// </summary>
        /// <param name="walletAddress">Wallet address</param>
        /// <param name="qrFileStream">QR file</param>
        /// <returns></returns>
        Task<string> SaveQrFile(string walletAddress, Stream qrFileStream);
    }
}
