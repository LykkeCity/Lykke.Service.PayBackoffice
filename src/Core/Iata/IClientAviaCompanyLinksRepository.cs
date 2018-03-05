using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Iata
{
    public interface IClientAviaCompanyLink
    {
        string ClientId { get; }
        string AviaCompanyId { get; }
    }

    public interface IClientAviaCompanyLinksRepository
    {
        Task LinkAsync(string clientId, string aviaCompanyId);
        Task<string> GetAviaCompanyId(string clientId);
        Task ClearAsync(string clientId);


        Task<IEnumerable<IClientAviaCompanyLink>> GetAllAsync();

    }
}
