using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IBannedClientsRepository
    {
        Task BanClient(string clientId);
        Task UnBanClient(string clientId);

        Task<IEnumerable<string>> GetBannedClients(IEnumerable<string> clientIds = null);
        Task<bool> IsClientBannedWithCache(string clientId, int cacheMinutes);
        Task<bool> IsClientBanned(string clientId);
    }
}
