using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.SwiftCredentials
{
    public interface ISwiftCredentialsRepository
    {
        Task SaveCredentialsAsync(ISwiftCredentials credentials);

        Task<IEnumerable<ISwiftCredentials>> GetAllAsync();

        Task<ISwiftCredentials> GetCredentialsAsync(string regulatorId, string assetId = null);

        Task DeleteCredentialsAsync(string regulatorId, string assetId);
    }
}
