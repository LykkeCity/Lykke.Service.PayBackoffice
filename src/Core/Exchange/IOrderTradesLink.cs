using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Exchange
{
    public interface IOrderTradeLink
    {
        string OrderId { get; }
        string TradeId { get; }
    }

    public interface IOrderTradesLinkRepository
    {
        Task<IEnumerable<string>> GetTradesAsync(string orderId);
    }
}
