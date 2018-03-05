using AzureStorage;
using Core.MarginTrading;
using Core.MarginTrading.MarketMaker;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureRepositories.MarginTrading
{
    public class MarginTradingExternalExchangeRepository : IMarginTradingExternalExchangeRepository
    {
        private readonly INoSQLTableStorage<MarginTradingMarketMakerExchangeEntity> _exchangeStorage;

        public MarginTradingExternalExchangeRepository(INoSQLTableStorage<MarginTradingMarketMakerExchangeEntity> exchangeStorage)
        {
            _exchangeStorage = exchangeStorage;
        }

        public Task CreateMarketMakerExchange(IExternalExchange record)
        {
            var newEntity = MarginTradingMarketMakerExchangeEntity.Create(record);
            return _exchangeStorage.InsertOrReplaceAsync(newEntity);            
        }

        public async Task DeleteMarketMakerExchangeById(string id)
        {
            var existing = (await _exchangeStorage.GetDataAsync(x => x.Id == id)).FirstOrDefault();
            if (existing != null)
                await _exchangeStorage.DeleteAsync(existing);
            else throw new System.Exception("Invalid Id");
        }

        public async Task<IEnumerable<IExternalExchange>> GetAllMarketMakerExchanges()
        {
            return await _exchangeStorage.GetDataAsync(MarginTradingMarketMakerExchangeEntity.GeneratePartitionKey());
        }

        public async Task<IExternalExchange> GetMarketMakerExchangeById(string id)
        {
            return await _exchangeStorage.GetDataAsync(MarginTradingMarketMakerExchangeEntity.GeneratePartitionKey(), id);
        }
    }
}
