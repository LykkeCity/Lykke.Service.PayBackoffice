using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Exchange
{
    public interface IOrderBooksService
    {
        Task<IEnumerable<IOrderBook>> GetAllAsync();
        Task<IEnumerable<IOrderBook>> GetAsync(string assetPairId);
    }
}
