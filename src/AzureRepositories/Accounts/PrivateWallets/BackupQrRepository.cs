using System;
using System.IO;
using System.Threading.Tasks;
using AzureStorage;
using Core.Accounts.PrivateWallets;

namespace AzureRepositories.Accounts.PrivateWallets
{
    public class BackupQrRepository : IBackupQrRepository
    {
        private readonly IBlobStorage _blobStorage;
        private const string ContainerName = "privatewallet-backup-qr";
        public BackupQrRepository(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public async Task<string> SaveQrFile(string walletAddress, Stream qrFileStream)
        {
            var fileName = $"{Guid.NewGuid().ToString("N")}_{walletAddress}";
            var qrUrl = await _blobStorage.SaveBlobAsync(ContainerName, fileName, qrFileStream, true);

            return qrUrl;
        }
    }
}
