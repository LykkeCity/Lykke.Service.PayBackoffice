using System.Threading.Tasks;
using Core.Accounts;
using Core.Clients;
using System.Linq;
using Lykke.Service.ClientAccount.Client;

namespace LkeServices.Clients
{
    public class SrvBackup : ISrvBackup
    {
        private readonly IClientAccountClient _clientAccountService;
        private readonly IWalletsRepository _walletsRepository;

        public SrvBackup(IClientAccountClient clientAccountService,
            IWalletsRepository walletsRepository)
        {
            _clientAccountService = clientAccountService;
            _walletsRepository = walletsRepository;
        }

        public async Task<bool> IsBackupRequired(string clientId)
        {
            var backupSettingsTask = _clientAccountService.GetBackupAsync(clientId);
            var wallets = await _walletsRepository.GetAsync(clientId);
            return wallets.Any(x => x.Balance > 0) && !(await backupSettingsTask).BackupDone;
        }

        public async Task<bool> IsBackupRequiredWithoutWalletCheck(string clientId)
        {
            var backupSettingsTask = _clientAccountService.GetBackupAsync(clientId);
            var wallets = await _walletsRepository.GetAsync(clientId);
            return !(await backupSettingsTask).BackupDone;
        }
    }
}
