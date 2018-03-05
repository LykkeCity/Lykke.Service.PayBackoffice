using System.Threading.Tasks;

namespace Core.Accounts.PrivateWallets
{
    public interface IPrivateWalletBackupRecord
    {
        string WalletAddress { get; set; }
        string SecurityQuestion { get; set; }
        string PrivateKeyBackup { get; set; }
    }

    public class PrivateWalletBackupDto : IPrivateWalletBackupRecord
    {
        public PrivateWalletBackupDto(string walletAddress, string securityQuestion,
            string privateKeyBackup)
        {
            WalletAddress = walletAddress;
            SecurityQuestion = securityQuestion;
            PrivateKeyBackup = privateKeyBackup;
        }

        public string WalletAddress { get; set; }
        public string SecurityQuestion { get; set; }
        public string PrivateKeyBackup { get; set; }
    }

    public interface IPrivateWalletBackupRepository
    {
        Task SaveBackupAsync(IPrivateWalletBackupRecord privateWalletBackupRecord);
    }
}
