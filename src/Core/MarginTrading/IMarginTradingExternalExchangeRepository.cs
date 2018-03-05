using Core.MarginTrading.MarketMaker;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.MarginTrading
{
    public interface IMarginTradingExternalExchangeRepository
    {
        Task CreateMarketMakerExchange(IExternalExchange record);
        Task<IEnumerable<IExternalExchange>> GetAllMarketMakerExchanges();
        Task<IExternalExchange> GetMarketMakerExchangeById(string id);
        Task DeleteMarketMakerExchangeById(string id);
    }
}
