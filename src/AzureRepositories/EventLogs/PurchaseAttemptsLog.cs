using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.EventLogs;
using Core.Exchange;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.EventLogs
{
    public class MarketOrderLogItemEntity : TableEntity
    {

        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }

        public string ClientId => PartitionKey;
        public DateTime CreatedAt { get; set; }



        public string Id { get; set; }
        public double Volume { get; set; }
        public string BaseAsset { get; set; }
        public string AssetPairId { get; set; }


        public static MarketOrderLogItemEntity Create(IMarketOrder src)
        {
            return new MarketOrderLogItemEntity
            {
                PartitionKey = GeneratePartitionKey(src.ClientId),
                CreatedAt = src.CreatedAt,
                Volume = src.Volume,
                AssetPairId = src.AssetPairId,
                Id = src.Id
            };
        }
    }


    public class PurchaseAttemptsLog : IPurchaseAttemptsLog
    {
        private readonly INoSQLTableStorage<MarketOrderLogItemEntity> _tableStorage;

        public PurchaseAttemptsLog(INoSQLTableStorage<MarketOrderLogItemEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }


        public Task RegisterAsync(IMarketOrder marketOrder)
        {
            var newEntity = MarketOrderLogItemEntity.Create(marketOrder);
            return _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, marketOrder.CreatedAt, RowKeyDateTimeFormat.Short);
        }
    }
}
