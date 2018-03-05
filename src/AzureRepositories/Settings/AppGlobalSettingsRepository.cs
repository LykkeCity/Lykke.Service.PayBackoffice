using System;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Settings;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Settings
{
    public class AppGlobalSettingsEntity : TableEntity, IAppGlobalSettings
    {
        public static string GeneratePartitionKey()
        {
            return "Setup";
        }

        public static string GenerateRowKey()
        {
            return "AppSettings";
        }

        public static AppGlobalSettingsEntity Create(IAppGlobalSettings appGlobalSettings)
        {
            return new AppGlobalSettingsEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                DepositUrl = appGlobalSettings.DepositUrl,
                DebugMode = appGlobalSettings.DebugMode,
                DefaultIosAssetGroup = appGlobalSettings.DefaultIosAssetGroup,
                DefaultAssetGroupForOther = appGlobalSettings.DefaultAssetGroupForOther,
                MinVersionOnReview = appGlobalSettings.MinVersionOnReview,
                IsOnReview = appGlobalSettings.IsOnReview,
                IcoLkkSold = appGlobalSettings.IcoLkkSold,
                IsOnMaintenance = appGlobalSettings.IsOnMaintenance,
                LowCashOutLimit = appGlobalSettings.LowCashOutLimit,
                LowCashOutTimeoutMins = appGlobalSettings.LowCashOutTimeoutMins,
                MarginTradingEnabled = appGlobalSettings.MarginTradingEnabled,
                CashOutBlocked = appGlobalSettings.CashOutBlocked,
                BtcOperationsDisabled = appGlobalSettings.BtcOperationsDisabled,
                BitcoinBlockchainOperationsDisabled = appGlobalSettings.BitcoinBlockchainOperationsDisabled,
                LimitOrdersEnabled = appGlobalSettings.LimitOrdersEnabled,
                MarketOrderPriceDeviation = appGlobalSettings.MarketOrderPriceDeviation,
                BlockedAssetPairs = appGlobalSettings.BlockedAssetPairs,
                OnReviewAssetConditionLayer = appGlobalSettings.OnReviewAssetConditionLayer,
                IcoStartDtForWhitelisted = appGlobalSettings.IcoStartDtForWhitelisted,
                IcoStartDt = appGlobalSettings.IcoStartDt,
                ShowIcoBanner = appGlobalSettings.ShowIcoBanner
            };
        }


        public string DepositUrl { get; set; }
        public bool DebugMode { get; set; }
        public string DefaultIosAssetGroup { get; set; }
        public string DefaultAssetGroupForOther { get; set; }
        public bool IsOnReview { get; set; }
        public double? MinVersionOnReview { get; set; }
        public double IcoLkkSold { get; set; }
        public bool IsOnMaintenance { get; set; }
        public int LowCashOutTimeoutMins { get; set; }
        public int LowCashOutLimit { get; set; }
        public bool MarginTradingEnabled { get; set; }
        public bool CashOutBlocked { get; set; }
        public bool BtcOperationsDisabled { get; set; }
        public bool BitcoinBlockchainOperationsDisabled { get; set; }
        public bool LimitOrdersEnabled { get; set; }
        public double MarketOrderPriceDeviation { get; set; }

        public DateTime? IcoStartDtForWhitelisted { get; set; }
        public DateTime? IcoStartDt { get; set; }
        public bool ShowIcoBanner { get; set; }

        public string[] BlockedAssetPairs
        {
            get { return BlockedAssetPairsJson.DeserializeJson<string[]>(); }
            set { BlockedAssetPairsJson = value.ToJson(); }
        }

        public string OnReviewAssetConditionLayer { get; set; }
        public string BlockedAssetPairsJson { get; set; }
    }

    public class AppGlobalSettingsRepository : IAppGlobalSettingsRepositry
    {

        private readonly INoSQLTableStorage<AppGlobalSettingsEntity> _tableStorage;

        public AppGlobalSettingsRepository(INoSQLTableStorage<AppGlobalSettingsEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task SaveAsync(IAppGlobalSettings appGlobalSettings)
        {
            var newEntity = AppGlobalSettingsEntity.Create(appGlobalSettings);
            return _tableStorage.InsertOrMergeAsync(newEntity);
        }

        public async Task UpdateAsync(string depositUrl = null, bool? debugMode = null,
            string defaultIosAssetGroup = null, string defaultAssetGroupForOther = null,
            double? minVersionOnReview = null, bool? isOnReview = null,
            double? icoLkkSold = null, bool? isOnMaintenance = null, int? lowCashOutTimeout = null,
            int? lowCashOutLimit = null, bool? marginTradingEnabled = null,
            bool? cashOutBlocked = null, bool? btcBlock = null, bool? btcBlockchainDisabled = null,
            bool? limitOrdersEnabled = null, double? marketOrdersDeviation = null, string[] blockedAssetPairs = null, 
            string onReviewAssetConditionLayer = null,
            DateTime? icoStartDtForWhitelisted = null, DateTime? icoStartDt = null, bool? showIcoBanner = null)
        {
            var entity =
                await
                    _tableStorage.GetDataAsync(AppGlobalSettingsEntity.GeneratePartitionKey(),
                        AppGlobalSettingsEntity.GenerateRowKey());

            entity.DebugMode = debugMode ?? entity.DebugMode;
            entity.DepositUrl = depositUrl;
            entity.DefaultIosAssetGroup = defaultIosAssetGroup;
            entity.DefaultAssetGroupForOther = defaultAssetGroupForOther;
            entity.MinVersionOnReview = minVersionOnReview ?? entity.MinVersionOnReview;
            entity.IsOnReview = isOnReview ?? entity.IsOnReview;
            entity.IcoLkkSold = icoLkkSold ?? entity.IcoLkkSold;
            entity.IsOnMaintenance = isOnMaintenance ?? entity.IsOnMaintenance;
            entity.LowCashOutLimit = lowCashOutLimit ?? entity.LowCashOutLimit;
            entity.LowCashOutTimeoutMins = lowCashOutTimeout ?? entity.LowCashOutTimeoutMins;
            entity.MarginTradingEnabled = marginTradingEnabled ?? entity.MarginTradingEnabled;
            entity.CashOutBlocked = cashOutBlocked ?? entity.CashOutBlocked;
            entity.BtcOperationsDisabled = btcBlock ?? entity.BtcOperationsDisabled;
            entity.BitcoinBlockchainOperationsDisabled = btcBlockchainDisabled ?? entity.BitcoinBlockchainOperationsDisabled;
            entity.LimitOrdersEnabled = limitOrdersEnabled ?? entity.LimitOrdersEnabled;
            entity.MarketOrderPriceDeviation = marketOrdersDeviation ?? entity.MarketOrderPriceDeviation;
            entity.BlockedAssetPairs = blockedAssetPairs;
            entity.OnReviewAssetConditionLayer = onReviewAssetConditionLayer ?? entity.OnReviewAssetConditionLayer;
            entity.IcoStartDtForWhitelisted = icoStartDtForWhitelisted ?? entity.IcoStartDtForWhitelisted;
            entity.IcoStartDt = icoStartDt ?? entity.IcoStartDt;
            entity.ShowIcoBanner = showIcoBanner ?? entity.ShowIcoBanner;

            await _tableStorage.InsertOrMergeAsync(entity);
        }

        public async Task<IAppGlobalSettings> GetAsync()
        {
            var partitionKey = AppGlobalSettingsEntity.GeneratePartitionKey();
            var rowKey = AppGlobalSettingsEntity.GenerateRowKey();
            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }




}
