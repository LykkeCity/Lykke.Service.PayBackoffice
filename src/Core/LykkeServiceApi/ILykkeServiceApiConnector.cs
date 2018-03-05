using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.LykkeServiceApi
{
    public interface ILykkeServiceApiConnector
    {
        Task<IEnumerable<TResult>> GetAsync<TResult>(string requestUrl);
    }
}