using System;
using System.Net;
using Autofac;
using AzureRepositories;
using BackOffice.Settings;
//using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Common;
using Common.Cache;
using Common.IocContainer;
using Common.Log;
//using Core;
//using Core.BcnReports;
//using Core.Bitcoin;
using Core.BitCoin;
//using Core.BitCoin.Ninja;
//using Core.Clients;
//using Core.Exchange;
//using Core.MarginTrading;
//using Core.Messages.Sms;
//using Core.Monitoring;
//using Core.Notifications;
//using Core.Offchain;
//using Core.PaymentSystems;
using Core.Security;
using LkeServices;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.Session;
//using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
//using LkeServices.Offchain;
//using Lykke.Service.Registration;
//using LkeDomain.Credentials;
//using LkeServices.Assets.AssetGroups;
//using LkeServices.BcnReports;
using LkeServices.Bitcoin;
//using LkeServices.CashoutRequests;
//using LkeServices.Clients;
//using LkeServices.Exchange;
//using LkeServices.Http;
//using LkeServices.Kyc;
//using LkeServices.MarginTrading;
//using LkeServices.Messages.Sms;
//using LkeServices.Monitoring;
//using LkeServices.Notifications;
//using LkeServices.PaymentSystems;
using LkeServices.Security;
//using LkeServices.Settings;
using Lykke.Service.AntifraudDetector;
//using Lykke.Service.Assets.Client;
//using Lykke.Service.ClientAssetRule.Client;
using Lykke.Service.ClientAccount.Client;
//using Lykke.Service.Limitations.Client;
//using Lykke.Service.ExchangeOperations.Client;
//using Lykke.Service.KycReports.Client;
//using Lykke.Service.Metrics.Client;
//using Lykke.Service.RateCalculator.Client;
//using Lykke.Service.Tracker.Client;
using Lykke.SettingsReader;
//using Lykke.Service.PersonalData.Client;
//using Lykke.Service.PersonalData.Contract;
//using Lykke.Service.Kyc.Abstractions.Services;
//using Lykke.Service.Kyc.Client;
//using Lykke.Service.Regulation.Client;
//using Lykke.Service.SeoReports.Client;
//using Lykke.Service.TemplateFormatter;
//using MarginTrading.DataReaderClient;
//using MarginTrading.MarketMaker.Contracts.Client;
//using Autofac.Extensions.DependencyInjection;
//using Lykke.Service.SwiftCredentials.Client;
//using MarginTrading.Backend.Contracts.Client;
//using MarginTrading.RiskManagement.HedgingService.Contracts.Client;
//using LkeServices.Strategy;
//using Core.Strategy;
//using Google.Apis.Http;
//using Lykke.Service.Assets.Client.Cache;
//using Lykke.Service.Assets.Client.Models;
//using Lykke.Service.AssetDisclaimers.Client;
//using Lykke.Service.FeeCalculator.Client;
//using MarginTrading.RiskManagement.RiskControlSystem.Contracts.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayAuth.Client;
using QBitNinja.Client;
using Lykke.Service.EmailPartnerRouter.Client;

namespace BackOffice.Binders
{
    public class AzureBinder : IDependencyBinder
    {
        internal static string BlockchainExplorerUrl;
        public ContainerBuilder Bind(IConfigurationRoot configuration, ContainerBuilder builder = null)
        {
            return Bind(configuration, builder, false);
        }

        public ContainerBuilder Bind(IConfigurationRoot configuration, ContainerBuilder builder = null, bool fromTest = false)
        {
            var settings = configuration.LoadSettings<BackOfficeBundle>();
            var monitoringServiceUrl = settings.CurrentValue.BackOffice.Service.MonitoringUrl;
            BlockchainExplorerUrl = settings.CurrentValue.BackOffice.BlockchainExplorerUrl;
            var ioc = builder ?? new ContainerBuilder();
            ioc.RegisterInstance(settings.CurrentValue.BackOffice);
            ioc.RegisterInstance(settings.CurrentValue.BackOffice.GoogleAuthSettings);
            //ioc.RegisterInstance(settings.CurrentValue.BackOffice.SwiftSettings);
            //ioc.RegisterInstance(settings.CurrentValue.BackOffice.CashoutSettings);
            ioc.RegisterInstance(settings.CurrentValue.BackOffice.DeploymentSettings);
            ioc.RegisterInstance(settings.CurrentValue.BackOffice.BitcoinCoreSettings);
            //ioc.RegisterInstance(settings.CurrentValue.BackOffice.CacheSettings);
            //ioc.RegisterInstance(settings.CurrentValue.BackOffice.PaymentSystems);
            ioc.RegisterInstance(settings.CurrentValue.BackOffice.LykkePayWalletList);
            //ioc.RegisterInstance(settings.CurrentValue.BackOffice.PdfGenerator);
            //ioc.RegisterInstance(settings.CurrentValue.BackOffice.SupportTools);
            ioc.RegisterInstance(settings.CurrentValue.BackOffice.TwoFactorVerification ?? new TwoFactorVerificationSettingsEx());
            //ioc.RegisterInstance(settings.CurrentValue.BackOffice.InternalTransfersSettings);
            //ioc.RegisterInstance(settings.CurrentValue.JumioServiceClient);
            //ioc.RegisterInstance(settings.CurrentValue.FeeSettings);

            Log = ioc.BindLog(settings.ConnectionString(x => x.BackOffice.Db.LogsConnString), "Backoffice", "LogBackoffce");

            Log.WriteInfoAsync("DResolver", "Binding", "", "Start");

            //var redis = new RedisCache(new RedisCacheOptions
            //{
            //    Configuration = settings.CurrentValue.BackOffice.CacheSettings.RedisConfiguration,
            //    InstanceName = settings.CurrentValue.BackOffice.CacheSettings.FinanceDataCacheInstance
            //});

            //ioc.RegisterInstance(redis).As<IDistributedCache>().SingleInstance();

            var cacheManager = new MemoryCacheManager();
            ioc.RegisterInstance<ICacheManager>(cacheManager);
            BindMicroservices(ioc, settings.CurrentValue, Log);
            ioc.BindAzureRepositories(settings.Nested(x => x.BackOffice.Db), settings.Nested(x => x.SmsNotifications), /*settings.ConnectionString(x => x.BackOffice.PdfGenerator.ConnectionString),*/ settings.CurrentValue.BackOffice.DefaultWithdrawalLimit, cacheManager, Log);
            ioc.BindBackOfficeRepositories(settings.Nested(x => x.BackOffice.Db), Log);

            BindBackOfficeServices(ioc, settings.CurrentValue);
            ioc.RegisterMonitoringServices(monitoringServiceUrl);
            //ioc.RegisterAllServices(settings.CurrentValue.BackOffice.SupportTools);

            //ioc.RegisterInstance<ISrvBlockchainReader>(new SrvNinjaBlockChainReader(settings.CurrentValue.NinjaServiceClient.ServiceUrl));
            //ioc.RegisterType<ClientOffchainExplorer>();
            //ioc.BindLykkeServicesApi(settings.CurrentValue.BackOffice.LykkeServiceApi);
            //ioc.RegisterMetricsClient(settings.CurrentValue.BackOffice.Service.MetricsUrl);
            //ioc.RegisterClientAssetRuleClient(settings.CurrentValue.ClientAssetRuleServiceClient);
            //ioc.RegisterRegulationClient(settings.CurrentValue.RegulationServiceClient.ServiceUrl);
            //ioc.RegisterSwiftCredentialsClient(settings.CurrentValue.SwiftCredentialsServiceClient);
            //ioc.RegisterInstance<IAssetDisclaimersClient>(new AssetDisclaimersClient(settings.CurrentValue.AssetDisclaimersServiceClient));
            //ioc.RegisterInstance<IPersonalDataService>(new PersonalDataService(settings.CurrentValue.PersonalDataServiceClient, Log));
            //ioc.RegisterGoogleDriveUploadService(settings.CurrentValue.BackOffice.GoogleDrive.GoogleDriveServiceUrl, Log);
            //ioc.RegisterInstance(settings.CurrentValue.BackOffice.GoogleDrive);

            //ioc.RegisterTemplateFormatter(settings.CurrentValue.BackOffice.Service.TemplateFormatterUrl, Log);

            //ioc.RegisterType<FeeCalculatorClient>()
            //    .WithParameter("serviceUrl", settings.CurrentValue.FeeCalculatorServiceClient.ServiceUrl)
            //    .WithParameter(TypedParameter.From(Log))
            //    .As<IFeeCalculatorClient>()
            //    .SingleInstance();

            ioc.RegisterInstance<IPayInternalClient>(new PayInternalClient(new PayInternalServiceClientSettings() { ServiceUrl = settings.CurrentValue.PayInternalServiceClient.ServiceUrl }));
            ioc.RegisterInstance<IPayInvoiceClient>(new PayInvoiceClient(new PayInvoiceServiceClientSettings() { ServiceUrl = settings.CurrentValue.PayInvoiceServiceClient.ServiceUrl }));
            ioc.RegisterInstance<IPayAuthClient>(new PayAuthClient(new PayAuthServiceClientSettings() { ServiceUrl = settings.CurrentValue.PayAuthServiceClient.ServiceUrl }, Log));
            ioc.RegisterInstance(new QBitNinjaClient(settings.CurrentValue.NinjaServiceClient.ServiceUrl)).AsSelf();
            ioc.RegisterInstance(new EmailPartnerRouterClient(settings.CurrentValue.EmailPartnerRouterServiceClient.ServiceUrl))
                .As<IEmailPartnerRouterClient>()
                .SingleInstance();

            Log.WriteInfoAsync("DResolver", "Binding", "", "App Stated");

            //ioc.RegisterInstance<IAssetsService>(new AssetsService(new Uri(settings.CurrentValue.BackOffice.Service.AssetsUrl)));

            //#region BLL dependecies

            //ioc.RegisterType<ClientAccountLogic>().SingleInstance();
            //#endregion

            if (settings.CurrentValue.BackOffice.MatchingEngine != null)
            {
#if DEBUG
                BindMatchingEngineChannel(
                    ioc,
                    fromTest
                        ? new IPEndPoint(8888, 8888)
                        : settings.CurrentValue.BackOffice.MatchingEngine.IpEndpoint.GetClientIpEndPoint());
#else
                BindMatchingEngineChannel(
                    ioc,
                    fromTest
                        ? new IPEndPoint(8888, 8888)
                        : settings.CurrentValue.BackOffice.MatchingEngine.IpEndpoint.GetClientIpEndPoint(true));
#endif
            }

            Log.WriteInfoAsync("DResolver", "Binding", "", "App Stated");

            return ioc;
        }

        public static void BindBackOfficeServices(ContainerBuilder ioc, BackOfficeBundle settings)
        {
            //ioc.RegisterType<SrvClientFinder>().SingleInstance();
            ioc.RegisterType<ClientSigningService>().As<IClientSigningService>().SingleInstance();
            //ioc.RegisterType<SrvBlockchainHelper>().As<ISrvBlockchainHelper>().SingleInstance();
            //ioc.RegisterType<HttpRequestClient>();
            //ioc.BindCachedDicts();
            //ioc.RegisterClientServices(settings.PersonalDataServiceClient);
            //var exchangeOperationsService = new ExchangeOperationsServiceClient(settings.ExchangeOperationsServiceClient.ServiceUrl);
            //ioc.RegisterInstance(exchangeOperationsService).As<IExchangeOperationsServiceClient>().SingleInstance();

            //ioc.RegisterLykkeServiceClient(settings.ClientAccountServiceClient.ServiceUrl);

            //ioc.RegisterType<SrvIpGeolocation>().As<ISrvIpGetLocation>().SingleInstance();

            //ioc.RegisterInstance(settings.KycServiceClient);
            //ioc.RegisterType<KycStatusServiceClient>().As<IKycStatusService>().SingleInstance();
            //ioc.RegisterType<KycDocumentsServiceClient>().As<IKycDocumentsService>().SingleInstance();
            //ioc.RegisterType<KycProfileServiceClient>().As<IKycProfileService>().SingleInstance();
            //ioc.RegisterType<KycInProgressServiceClient>().As<IKycInProgressService>().SingleInstance();
            //ioc.RegisterType<KycTempatesServiceClient>().As<IKycTempatesService>().SingleInstance();
            //ioc.RegisterType<SrvKycManager>().SingleInstance();

            //ioc.RegisterType<OrderBookService>().As<IOrderBooksService>().SingleInstance();
            //ioc.Register<IAppNotifications>(x => new SrvAppNotifications(settings.BackOffice.Jobs.NotificationsHubConnectionString,
            //    settings.BackOffice.Jobs.NotificationsHubName));

            //ioc.RegisterType<QueueSmsRequestProducer>().As<ISmsRequestProducer>().SingleInstance();

            //ioc.RegisterInstance(settings.BackOffice.CacheSettings);
            //ioc.RegisterType<SrvAssetsHelper>().SingleInstance();

            ioc.RegisterType<SrvSecurityHelper>().As<ISrvSecurityHelper>().SingleInstance();
            //ioc.RegisterType<SrvPaymentProcessor>().SingleInstance();
            //ioc.RegisterType<SrvIcoLkkSoldCounter>().SingleInstance();

            //ioc.Register<IBitcoinApiClient>(x => new BitcoinApiClient(settings.BackOffice.BitcoinCoreSettings.BitcoinCoreApiUrl)).SingleInstance();

            //ioc.RegisterInstance(settings.BackOffice.MarginSettings);            
            //ioc.Register(context =>
            //      MarginTradingDataReaderApiClientFactory.CreateDefaultClientsPair(
            //          settings.BackOffice.MarginSettings.DataReaderDemoApiUrl,
            //          settings.BackOffice.MarginSettings.DataReaderLiveApiUrl,
            //          settings.BackOffice.MarginSettings.DataReaderDemoApiKey,
            //          settings.BackOffice.MarginSettings.DataReaderLiveApiKey,
            //          "LykkeWallet.Backoffice"))
            //          .SingleInstance();
            //ioc.RegisterType<MarginDataServiceResolver>().As<IMarginDataServiceResolver>().SingleInstance();
            //ioc.RegisterType<PaymentSystemFacade>().As<IPaymentSystemFacade>();
            //ioc.RegisterType<OffchainRequestService>().As<IOffchainRequestService>();
            //ioc.RegisterType<MonitoringServiceCallerService>().As<IMonitoringServiceCallerService>().SingleInstance();
            //var addressTransactionsReportsSettings = new AddressTransactionsReportsSettings
            //{
            //    BaseUri = settings.BackOffice.BcnReports.ApiUrl
            //};
            //ioc.RegisterInstance(addressTransactionsReportsSettings).SingleInstance();
            //ioc.RegisterType<AddressTransactionsReportsService>().As<IAddressTransactionsReportsService>();

            //var strategiesSettings = new StrategiesSettings
            //{
            //    BaseUri = settings.StrategiesSettings.BaseUri
            //};
            //ioc.RegisterInstance(strategiesSettings).SingleInstance();
            //ioc.RegisterType<MmApiClient>().As<IMmApiClient>();
            //ioc.RegisterType<StrategyService>().As<IStrategyService>();

            //var assetTransactionsReportsSettings = new AssetTransactionsReportsSettings
            //{
            //    BaseUri = settings.BackOffice.BcnReports.ApiUrl
            //};
            //ioc.RegisterInstance(assetTransactionsReportsSettings).SingleInstance();
            //ioc.RegisterType<AssetTransactionsReportsService>().As<IAssetTransactionsReportsService>();

            //var blockTransactionsReportSettings = new BlockTransactionsReportSettings
            //{
            //    BaseUri = settings.BackOffice.BcnReports.ApiUrl
            //};
            //ioc.RegisterInstance(blockTransactionsReportSettings).SingleInstance();
            //ioc.RegisterType<BlockTransactionsReportsService>().As<IBlockTransactionsReportsService>();
            //ioc.RegisterType<ConfirmationCodesService>().As<IConfirmationCodesService>();
            //ioc.RegisterType<CashoutRequestsManager>().SingleInstance();
            
            Microsoft.Extensions.DependencyInjection.IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            //services.RegisterMtBackendClientsPair(
            //    settings.BackOffice.MarginSettings.DemoApiRootUrl,
            //    settings.BackOffice.MarginSettings.ApiRootUrl,
            //    settings.BackOffice.MarginSettings.DemoApiKey,
            //    settings.BackOffice.MarginSettings.ApiKey,
            //    "LykkeWallet.Backoffice");
            
            //services.RegisterMtMarketMakerClient(settings.BackOffice.MarginSettings.MarketMakerApiUrl, "LykkeWallet.Backoffice");
            //services.RegisterMtRiskManagementRiskControlSystemClientsPair(
            //    settings.RiskManagementServiceClient.DemoUri,
            //    settings.RiskManagementServiceClient.DemoApiKey,
            //    settings.RiskManagementServiceClient.BaseUri,
            //    settings.RiskManagementServiceClient.ApiKey,
            //    "LykkeWallet.Backoffice");

            //services.RegisterMtRiskManagementHedgingServiceClientsPair(
            //    settings.RiskManagementServiceClient.HedgingServiceDemoUrl,
            //    settings.RiskManagementServiceClient.HedgingServiceDemoApiKey,
            //    settings.RiskManagementServiceClient.HedgingServiceUrl,
            //    settings.RiskManagementServiceClient.HedgingServiceApiKey,
            //    "LykkeWallet.Backoffice");
            //ioc.Populate(services);
            ioc.RegisterLykkeServiceClient(settings.ClientAccountServiceClient.ServiceUrl);

            //var assetsCache = new DictionaryCache<Asset>(
            //    new DateTimeProvider(),
            //    TimeSpan.FromSeconds(settings.BackOffice.InternalTransfersSettings.AssetsCacheExpirationSeconds)
            //);

            //var assetsService = new AssetsService(new Uri(settings.BackOffice.Service.AssetsUrl));
            //var assetsServiceWithCache = new AssetsServiceWithCache(assetsService, assetsCache, null);
            //ioc.RegisterInstance(assetsServiceWithCache).AsImplementedInterfaces().SingleInstance();
        }

        public ILog Log { get; set; }

        private void BindMatchingEngineChannel(ContainerBuilder ioc, IPEndPoint ipEndPoint)
        {
            var socketLog = new SocketLogDynamic(i => { },
                str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));

            //ioc.BindMeClient(ipEndPoint, socketLog);
        }

        private static void BindMicroservices(ContainerBuilder container, BackOfficeBundle allSettings, ILog log)
        {
            BackOfficeServiceSettings settings = allSettings.BackOffice.Service;
            
            container.RegisterClientSessionService(settings.SessionUrl, log);
            //container.RegisterSeoReportsClient(settings.SeoReportsUrl, log);
            //container.RegisterRegistrationClient(settings.RegistrationUrl, log);
            container.RegisterAntifraudDetectorClient(settings.AntifraudDetectorUrl, log);
            //container.RegisterKycReportsClient(settings.KycReportsServiceUrl, log);
            //container.RegisterLimitationsServiceClient(settings.LimitationsServiceUrl);
            //container.RegisterTrackerClient(settings.TrackerServiceApiUrl, log);
            //container.RegisterRateCalculatorClient(settings.RateCalculatorServiceApiUrl, log);
            //container.RegisterInstance<IAssetsService>(new AssetsService(new Uri(settings.AssetsUrl)));
            //container.RegisterFeeCalculatorClient(allSettings.FeeCalculatorServiceClient.ServiceUrl, log);
        }
    }
}
