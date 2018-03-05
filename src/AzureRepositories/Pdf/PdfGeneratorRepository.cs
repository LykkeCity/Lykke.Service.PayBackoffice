using AzureStorage;
using Common.Log;
using Core.Pdf;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Threading.Tasks;
using Common;

namespace AzureRepositories.Pdf
{
    public class PdfGeneratorRepository : IPdfGeneratorRepository
    {
        private readonly IBlobStorage _storage;
        private readonly ILog _log;
        
        public PdfGeneratorRepository(IBlobStorage storage, ILog log)
        {
            _storage = storage;
            _log = log;
        }
        
        public async Task<(byte[] Data, string FileName)> GetDataAsync(string containerName, string blobName)
        {
            byte[] data = null;

            try
            {
                using (var stream = await _storage.GetAsync(containerName, blobName))
                {
                    data = await stream.ToBytesAsync();
                }
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                await _log.WriteInfoAsync("PdfGeneratorRepository.GetDataAsync", (new { containerName, blobName }).ToJson(), $"Cannot get data from storage: {ex.Message}");
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("PdfGeneratorRepository.GetDataAsync", (new { containerName, blobName }).ToJson(), ex);
            }

            if (data == null)
            {
                return (Data: null, FileName: string.Empty);
            }

            var fileName = await _storage.GetMetadataAsync(containerName, blobName, "fileName");
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "cashout report.pdf";
            }

            var result = (Data: data, FileName: fileName);
            return result;
        }

        public async Task<bool> IsExists(string containerName, string blobName)
        {
            try
            {
                return await _storage.HasBlobAsync(containerName, blobName);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("PdfGeneratorRepository.IsExists", new { containerName, blobName }.ToJson(), ex);
                return false;
            }
        }
    }
}
