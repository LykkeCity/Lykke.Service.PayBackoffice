using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.Wallets;

namespace Core.Settings
{
    public class IpEndpointSettings
    {
        public string InternalHost { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public IPEndPoint GetClientIpEndPoint(bool useInternal = false)
        {
            string host = useInternal ? InternalHost : Host;
            IPAddress address;
            if (!IPAddress.TryParse(host, out address))
                address = Dns.GetHostAddressesAsync(host).Result[0];
            return new IPEndPoint(address, Port);
        }

        public IPEndPoint GetServerIpEndPoint()
        {
            return new IPEndPoint(IPAddress.Any, Port);
        }

    }

    public class DbSettings
    {
        public string ClientPersonalInfoConnString { get; set; }
        public string BalancesInfoConnString { get; set; }
        public string HMarketOrdersConnString { get; set; }
        public string HTradesConnString { get; set; }
        public string HLiquidityConnString { get; set; }
        public string BackOfficeConnString { get; set; }
        public string BitCoinQueueConnectionString { get; set; }
        public string DictsConnString { get; set; }
        public string LogsConnString { get; set; }
        public string SharedStorageConnString { get; set; }
        public string OlapConnString { get; set; }
        public string ChronoBankSrvConnString { get; set; }
        public string QuantaSrvConnString { get; set; }
        public string ClientSignatureConnString { get; set; }
        public string SolarCoinConnString { get; set; }
        public string OffchainConnString { get; set; }
        public string ClientBalanceLogsConnString { get; set; }
        public string ApiLogsConnString { get; set; }
        public string LwEthLogsConnString { get; set; }
        public string SecurityEventsConnString { get; set; }
        public string ReportsServiceConnString { get; set; }
        public string MarginTradingFrontendConnString { get; set; }
        public string InternalTransactionsConnectionString { get; set; }
        public string AlphaEngineAuditConnString { get; set; }
    }


    public class MatchingOrdersSettings
    {
        public IpEndpointSettings IpEndpoint { get; set; }
    }

    public class JobsSettings
    {
        public string NotificationsHubName { get; set; }
        public string NotificationsHubConnectionString { get; set; }
    }

    /// <summary>
    /// System parameters for FxPaygate site API
    /// </summary>
    public class FxpaygateSettings
    {
        public string[] Currencies { get; set; }
        public string[] Countries { get; set; }
        public Dictionary<OwnerType, string> ServiceUrls { get; set; }
        public string[] SupportedCurrencies { get; set; }
    }

    /// <summary>
    /// system parameters for Credit voucers API. See https://www.creditvouchers.com/site/getapi.html
    /// </summary>
    public class CreditVouchersSettings
    {
        public double MinAmount { get; set; }

        public double MaxAmount { get; set; }

        public string[] ServiceUrls { get; set; }

        public string[] SupportedCurrencies { get; set; }
    }

    public class PaymentSystemsSettings
    {
        public CreditVouchersSettings CreditVouchers { get; set; }
        public FxpaygateSettings Fxpaygate { get; set; }
    }
    public class LykkePayWalletListSettings
    {
        public LykkePayWalletSettings[] Wallets { get; set; }
    }
    public class LykkePayWalletSettings
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
    public class SwiftSettings
    {
        /// <summary>
        /// Wallet of this client is used for cash out for cash in operations for others
        /// </summary>
        public string SwiftTransferSourceClientId { get; set; }
    }

    public class ExternalLinksSettings
    {
        public string TermsOfUse { get; set; }
        public string InformationBrochure { get; set; }
        public string RefundInfo { get; set; }
        public string SupportPhoneNum { get; set; }
        public string UserAgreementUrl { get; set; }
    }

    public class ExchangeSettings
    {
        public double MinBtcOrderAmount { get; set; }
        public decimal MaxLimitOrderDeviationPercent { get; set; }
    }

    public static class ChannelTypes
    {
        public const string Errors = "Errors";
        public const string Warnings = "Warnings";
        public const string Info = "Info";
        public const string CashFlow = "CashFlow";
        public const string MarginTrading = "MarginTrading";
    }

    public class SlackIntegrationSettings
    {
        public class Channel
        {
            public string Type { get; set; }
            public string WebHookUrl { get; set; }
        }

        public string Env { get; set; }
        public Channel[] Channels { get; set; }
    }

    public static class SlackIntegrationSettingsExt
    {
        public static string GetChannelWebHook(this SlackIntegrationSettings settings, string type)
        {
            return settings.Channels.FirstOrDefault(x => x.Type == type)?.WebHookUrl;
        }
    }

    public class TwilioSettings
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string SwissNumber { get; set; }
        public string UsNumber { get; set; }
    }

    public class LykkeServiceApiSettings
    {
        public string ServiceUri { get; set; }
    }

    public class ChronoBankServiceClientSettings
    {
        public string ServiceUrl { get; set; }
    }


    public class BitcoinCoreSettings
    {
        public string BitcoinCoreApiUrl { get; set; }
        public string ClientSigningApiUrl { get; set; }
    }

    public class TaxReportsSettings
    {
        public string HotWalletId { set; get; }
    }

    public class CacheSettings
    {
        public string FinanceDataCacheInstance { get; set; }
        public string RedisConfiguration { get; set; }

        public string OrderBooksCacheKeyPattern { get; set; }
    }

    public static class CacheSettingsExt
    {
        public static string GetOrderBookKey(this CacheSettings settings, string assetPairId, bool isBuy)
        {
            return string.Format(settings.OrderBooksCacheKeyPattern, assetPairId, isBuy);
        }
    }

    public class MarginSettings
    {
        public string MarginTradingWampHost { get; set; }
        public string ApiKey { get; set; }
        public string DemoApiKey { get; set; }
        public string ApiUrl { get; set; }
        public string ApiRootUrl { get; set; }
        public string DemoApiRootUrl { get; set; }

        public string MoneyTrasferClientId { get; set; }

        public string MarketMakerApiUrl { get; set; }

        public string DataReaderDemoApiUrl { get; set; }
        public string DataReaderLiveApiUrl { get; set; }
        public string DataReaderDemoApiKey { get; set; }
        public string DataReaderLiveApiKey { get; set; }
    }

    public class QuantaServiceClientSettings
    {
        public string ServiceUrl { get; set; }
    }

    public class OffchainSettings
    {
        public string HotWalletAddress { get; set; }
        public int DepositRequestLockSeconds { get; set; }
        public string BccWalletAddress { get; set; }
    }

    public class EthereumSettings
    {
        public string EthereumCoreUrl { get; set; }
        public string HotwalletAddress { get; set; }
    }

    public class GoogleAuthSettings
    {
        public string ApiClientId { get; set; }

        public string AvailableEmailsRegex { get; set; }

        public string DefaultAdminEmail { get; set; }
    }

    public class ServiceSettings
    {
        public string SessionUrl { get; set; }
        public string LimitationsServiceUrl { get; set; }
        public string RegistrationUrl { get; set; }
        public string AntifraudDetectorUrl { get; set; }
        public string TrackerServiceApiUrl { get; set; }
        public string RateCalculatorServiceApiUrl { get; set; }
        public string OperationsUrl { get; set; }
        public string AssetsServiceUrl { get; set; }
    }

    public class BcnReportsSettings
    {
        public string ApiUrl { get; set; }
    }

    public class PdfGeneratorSettings
    {
        public string ServiceUrl { get; set; }
        public string ConnectionString { get; set; }
    }

    public class GoogleDriveSettings
    {
        public string GoogleDriveServiceUrl { get; set; }
        public string SwiftCashoutReportsFolderId { get; set; }
        public string UsdFolderId { get; set; }
        public string EurFolderId { get; set; }
        public string GbpFolderId { get; set; }
        public string ChfFolderId { get; set; }
        public string OthersFolderId { get; set; }
    }

    public class SupportToolsSettings
    {
        public int PriorityCodeExpirationInterval { get; set; }
    }

    public class RiskManagementServiceClientSettings
    {
        public string BaseUri { get; set; }
        public string ApiKey { get; set; }
        public string DemoUri { get; set; }
        public string DemoApiKey { get; set; }

        [Lykke.SettingsReader.Attributes.Optional]
        public string HedgingServiceUrl { get; set; }
        [Lykke.SettingsReader.Attributes.Optional]
        public string HedgingServiceApiKey { get; set; }
        [Lykke.SettingsReader.Attributes.Optional]
        public string HedgingServiceDemoUrl { get; set; }
        [Lykke.SettingsReader.Attributes.Optional]
        public string HedgingServiceDemoApiKey { get; set; }
    }

    public class MicrographCacheServiceClientSettings
    {
        public string ServiceUrl { get; set; }
    }

    public class IcoSettings
    {
        public string[] RestrictedCountriesIso3 { get; set; }
        public string CheckWhitelistedUrl { get; set; }
        // ReSharper disable once InconsistentNaming
        public string LKK2YAssetId { get; set; }
    }

    public static class IcoSettingsExt
    {
        public static bool IsRestricted(this IcoSettings icoSettings, string countryIso3)
        {
            return countryIso3 != null && (icoSettings.RestrictedCountriesIso3?.Contains(countryIso3) ?? false);
        }
    }
}
