using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pdf
{
    public interface IPdfGeneratorRepository
    {
        Task<(byte[] Data, string FileName)> GetDataAsync(string containerName, string blobName);
        Task<bool> IsExists(string containerName, string blobName);
    }
}
