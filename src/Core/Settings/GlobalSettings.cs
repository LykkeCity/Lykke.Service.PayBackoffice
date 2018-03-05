using System;
using System.Threading.Tasks;

namespace Core.Settings
{
    public static class GlobalSettings
    {
        public const int Mt4TimeOffset = 2;
        public const string AssetAccuracyMask = "0.##########";



        public static string ToAssetMultiplierString(this double src)
        {
            return src.ToString(AssetAccuracyMask);
        }
    }


    public interface IAppGlobalSettings
    {
        string DepositUrl { get; }
        bool DebugMode { get; }
        string DefaultIosAssetGroup { get; set; }
        string DefaultAssetGroupForOther { get; set; }
        bool IsOnReview { get; }
        double? MinVersionOnReview { get; }
        double IcoLkkSold { get; }
        bool IsOnMaintenance { get; }
        int LowCashOutTimeoutMins { get; }
        int LowCashOutLimit { get; }
        bool MarginTradingEnabled { get; }
        bool CashOutBlocked { get; }
        bool BtcOperationsDisabled { get; }
        bool BitcoinBlockchainOperationsDisabled { get; }
        bool LimitOrdersEnabled { get; }
        double MarketOrderPriceDeviation { get; }
        string[] BlockedAssetPairs { get; set; }
        string OnReviewAssetConditionLayer { get; set; }
        DateTime? IcoStartDtForWhitelisted { get; set; }
        DateTime? IcoStartDt { get; set; }
        bool ShowIcoBanner { get; set; }
    }

    public class AppGlobalSettings : IAppGlobalSettings
    {
        public static AppGlobalSettings CreateDefault()
        {
            return new AppGlobalSettings
            {
                DepositUrl = "http://mock-bankcards.azurewebsites.net/",
                DebugMode = true
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
        public string[] BlockedAssetPairs { get; set; }
        public string OnReviewAssetConditionLayer { get; set; }
        public DateTime? IcoStartDtForWhitelisted { get; set; }
        public DateTime? IcoStartDt { get; set; }
        public bool ShowIcoBanner { get; set; }
    }

    public interface IAppGlobalSettingsRepositry
    {
        Task SaveAsync(IAppGlobalSettings appGlobalSettings);

        Task UpdateAsync(string depositUrl = null, bool? debugMode = null,
            string defaultIosAssetGroup = null, string defaultAssetGroupForOther = null,
            double? minVersionOnReview = null, bool? isOnReview = null,
            double? icoLkkSold = null, bool? isOnMaintenance = null, int? lowCashOutTimeout = null,
            int? lowCashOutLimit = null, bool? marginTradingEnabled = null,
            bool? cashOutBlocked = null, bool? btcDisabled = null, bool? btcBlockchainDisabled = null,
            bool? limitOrdersEnabled = null, double? marketOrdersDeviation = null, string[] blockedAssetPairs = null,
            string onReviewAssetConditionLayer = null,
            DateTime? icoStartDtForWhitelisted = null, DateTime? icoStartDt = null, bool? showIcoBanner = null);

        Task<IAppGlobalSettings> GetAsync();
    }


    public static class AppGlobalSettingsRepositry
    {
        public static async Task<IAppGlobalSettings> GetFromDbOrDefault(this IAppGlobalSettingsRepositry table)
        {
            return await table.GetAsync() ?? AppGlobalSettings.CreateDefault();
        }
    }

}
