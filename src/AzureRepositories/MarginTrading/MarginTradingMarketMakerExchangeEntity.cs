using Core.MarginTrading.MarketMaker;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureRepositories.MarginTrading
{
    public class MarginTradingMarketMakerExchangeEntity : TableEntity, IExternalExchange
    {
        public string Id { get; set; }
        public string Name { get; set; }

        DateTime IExternalExchange.Timestamp => Timestamp.DateTime;

        internal static string GeneratePartitionKey()
        {
            return "MMExternalExchange";
        }
        internal static string GenerateRowkey(string id)
        {
            return id;
        }

        public static MarginTradingMarketMakerExchangeEntity Create(IExternalExchange src)
        {
            return new MarginTradingMarketMakerExchangeEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowkey(src.Id),
                Id = src.Id,
                Name = src.Name,
                Timestamp = src.Timestamp
            };
        }
    }
}
