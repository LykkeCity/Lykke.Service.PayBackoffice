using System;

namespace Core
{
    public enum NetworkType
    {
        Main,
        Testnet
    }

    public static class LykkeConstants
    {
        public const string Iso2USA = "US";
        public const string Iso3USA = "USA";
        public const string BitcoinAssetId = "BTC";
        public const string LykkeAssetId = "LKK";
        public const string LykkeForwardAssetId = "LKK1Y";

        public const string UsdAssetId = "USD";
        public const string EurAssetId = "EUR";
        public const string ChfAssetId = "CHF";
        public const string GbpAssetId = "GBP";
        public const string EthAssetId = "ETH";
        public const string SolarAssetId = "SLR";
        public const string ChronoBankAssetId = "TIME";
        public const string QuantaAssetId = "QNT";

        public const string LKKUSDPairId = "LKKUSD";

        public const string DefaultAssetSetting = "Default";

        public const int TotalLykkeAmount = 1250000000;

        public const int MinPwdLength = 6;
        public const int MaxPwdLength = 100;

        public const int DefaultRefundTimeoutDays = 30;

        public static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(3);
        public static readonly TimeSpan SessionRefreshPeriod = TimeSpan.FromDays(1);
        public const string BcnKeyForErc223 = "ERC223";

        [Obsolete]
        public const string BcnKeyForErc20Old = "ERC20";

        public static readonly Guid TrustedBtcDialogId = Guid.Parse("e81f5087-f792-48e9-9ffa-0e4f8f9d5179");
        public static readonly Guid TrustedBtcButtonOk = Guid.Parse("e81f5087-f792-48e9-9ffa-0e4f8f9d5171");
        public static readonly Guid TrustedBtcButtonCancel = Guid.Parse("e81f5087-f792-48e9-9ffa-0e4f8f9d5170");

        #region Cache keys

        public const string LastAskBidForAssetOnPeriod = "__Asset_{0}_Last_ask{1}_{2}__";

        #endregion

        #region SMS Providers

        public const string Twilio = "Twilio";
        public const string Nexmo = "Nexmo";

        #endregion

        public static string GetLastAskForAssetOnPeriodKey(string assetPairId, string period, bool ask)
        {
            return string.Format(LastAskBidForAssetOnPeriod, assetPairId, ask, period);
        }
    }
}
