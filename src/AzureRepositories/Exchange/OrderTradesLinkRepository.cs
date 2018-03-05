using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Exchange;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Exchange
{
    public class OrderTradeLinkEntity : TableEntity, IOrderTradeLink
    {
        public string OrderId => PartitionKey;
        public string TradeId => RowKey;
    }

    public class OrderTradesLinkRepository : IOrderTradesLinkRepository
    {
        readonly INoSQLTableStorage<OrderTradeLinkEntity> _tableStorage;

        public OrderTradesLinkRepository(INoSQLTableStorage<OrderTradeLinkEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<string>> GetTradesAsync(string orderId)
        {
            return (await _tableStorage.GetDataAsync(orderId)).Select(x => x.TradeId);
        }
    }
}
