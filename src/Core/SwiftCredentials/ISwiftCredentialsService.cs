using System.Threading.Tasks;
using Core.Clients;

namespace Core.SwiftCredentials
{
    public interface ISwiftCredentialsService
    {
        Task<ISwiftCredentials> GetDefaultCredentialsAsync(string assetId);

        Task<ISwiftCredentials> GetCredentialsAsync(string assetId, string clientId);
        Task<ISwiftCredentials> GetCredentialsAsync(string assetId, string clientSpotRegulator, string clientEmail);
    }
}
