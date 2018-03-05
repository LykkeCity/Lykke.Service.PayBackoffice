using System.Threading.Tasks;
using AzureStorage;
using Core.Exchange;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Exchange
{

    public class ExchangeSettingsEntity : TableEntity, IExchangeSettings
    {

        public static string GeneratePartitionKey( )
        {
            return "ExchngSettings";
        }

        public static string GenerateRowKey(string cleintId )
        {
            return cleintId;
        }

        public string BaseAssetIos { get; set; }
        public string BaseAssetOther { get; set; }
        public bool SignOrder { get; set; }

        public static ExchangeSettingsEntity CreateEmpty(string clientId, IExchangeSettings src)
        {
            return new ExchangeSettingsEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(clientId),
                BaseAssetIos = src.BaseAssetIos,
                BaseAssetOther = src.BaseAssetOther,
                SignOrder = src.SignOrder
            };

        }

    }


    public class ExchangeSettingsRepository : IExchangeSettingsRepository
    {
        private readonly INoSQLTableStorage<ExchangeSettingsEntity> _tableStorage;

        public ExchangeSettingsRepository(INoSQLTableStorage<ExchangeSettingsEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task UpdateBaseAssetIosAsync(string clientId, string baseAsset)
        {
            var partitionKey = ExchangeSettingsEntity.GeneratePartitionKey();
            var rowKey = ExchangeSettingsEntity.GenerateRowKey(clientId);

            return _tableStorage.InsertOrModifyAsync(
                partitionKey,
                rowKey,
                () =>
                {
                    var entity = ExchangeSettings.CreateDeafault();
                    entity.BaseAssetIos = baseAsset;
                    return ExchangeSettingsEntity.CreateEmpty(clientId, entity);
                },
                entityToModify =>
                {
                    entityToModify.BaseAssetIos = baseAsset;
                    return entityToModify;
                }
                );
        }

        public Task UpdateBaseAssetOtherAsync(string clientId, string baseAsset)
        {
            var partitionKey = ExchangeSettingsEntity.GeneratePartitionKey();
            var rowKey = ExchangeSettingsEntity.GenerateRowKey(clientId);

            return _tableStorage.InsertOrModifyAsync(
                partitionKey,
                rowKey,
                () =>
                {
                    var entity = ExchangeSettings.CreateDeafault();
                    entity.BaseAssetOther = baseAsset;
                    return ExchangeSettingsEntity.CreateEmpty(clientId, entity);
                },
                entityToModify =>
                {
                    entityToModify.BaseAssetOther = baseAsset;
                    return entityToModify;
                }
                );
        }

        public Task UpdateSignOrderAsync(string clientId, bool value)
        {
            var partitionKey = ExchangeSettingsEntity.GeneratePartitionKey();
            var rowKey = ExchangeSettingsEntity.GenerateRowKey(clientId);

            return _tableStorage.InsertOrModifyAsync(
                partitionKey,
                rowKey,
                () =>
                {
                    var entity = ExchangeSettings.CreateDeafault();
                    entity.SignOrder = value;
                    return ExchangeSettingsEntity.CreateEmpty(clientId, entity);
                },
                entityToModify =>
                {
                    entityToModify.SignOrder = value;
                    return entityToModify;
                }
                );
        }

        public async Task<IExchangeSettings> GetAsync(string clientId)
        {
            var partitionKey = ExchangeSettingsEntity.GeneratePartitionKey();
            var rowKey = ExchangeSettingsEntity.GenerateRowKey(clientId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}
