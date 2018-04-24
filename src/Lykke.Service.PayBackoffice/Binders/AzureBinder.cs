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
using Lykke.Service.Balances.Client;
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
            var monitoringServiceUrl = settings.CurrentValue.PayBackOffice.Service.MonitoringUrl;
            BlockchainExplorerUrl = settings.CurrentValue.PayBackOffice.BlockchainExplorerUrl;
            var ioc = builder ?? new ContainerBuilder();
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice);
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.GoogleAuthSettings);
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.DeploymentSettings);
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.BitcoinCoreSettings);
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.TwoFactorVerification ?? new TwoFactorVerificationSettingsEx());
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.LykkePayWalletList);

            Log = ioc.BindLog(settings.ConnectionString(x => x.PayBackOffice.Db.LogsConnString), "Backoffice", "LogBackoffce");

            Log.WriteInfoAsync("DResolver", "Binding", "", "Start");

            var cacheManager = new MemoryCacheManager();
            ioc.RegisterInstance<ICacheManager>(cacheManager);
            BindMicroservices(ioc, settings.CurrentValue, Log);
            ioc.BindAzureRepositories(settings.Nested(x => x.PayBackOffice.Db), cacheManager, Log);
            ioc.BindBackOfficeRepositories(settings.Nested(x => x.PayBackOffice.Db), Log);

            BindBackOfficeServices(ioc, settings.CurrentValue);
            ioc.RegisterMonitoringServices(monitoringServiceUrl);

            ioc.RegisterInstance<IPayInternalClient>(new PayInternalClient(new PayInternalServiceClientSettings() { ServiceUrl = settings.CurrentValue.PayInternalServiceClient.ServiceUrl }));
            ioc.RegisterInstance<IPayInvoiceClient>(new PayInvoiceClient(new PayInvoiceServiceClientSettings() { ServiceUrl = settings.CurrentValue.PayInvoiceServiceClient.ServiceUrl }));
            ioc.RegisterInstance<IPayAuthClient>(new PayAuthClient(new PayAuthServiceClientSettings() { ServiceUrl = settings.CurrentValue.PayAuthServiceClient.ServiceUrl }, Log));
            ioc.RegisterInstance(new QBitNinjaClient(settings.CurrentValue.NinjaServiceClient.ServiceUrl)).AsSelf();
            ioc.RegisterInstance(new EmailPartnerRouterClient(settings.CurrentValue.EmailPartnerRouterServiceClient.ServiceUrl))
                .As<IEmailPartnerRouterClient>()
                .SingleInstance();

            Log.WriteInfoAsync("DResolver", "Binding", "", "App Stated");

            return ioc;
        }

        public static void BindBackOfficeServices(ContainerBuilder ioc, BackOfficeBundle settings)
        {
            ioc.RegisterType<ClientSigningService>().As<IClientSigningService>().SingleInstance();

            ioc.RegisterType<SrvSecurityHelper>().As<ISrvSecurityHelper>().SingleInstance();
            
            Microsoft.Extensions.DependencyInjection.IServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            ioc.RegisterLykkeServiceClient(settings.ClientAccountServiceClient.ServiceUrl);
        }

        public ILog Log { get; set; }

        private void BindMatchingEngineChannel(ContainerBuilder ioc, IPEndPoint ipEndPoint)
        {
            var socketLog = new SocketLogDynamic(i => { },
                str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));
        }

        private static void BindMicroservices(ContainerBuilder container, BackOfficeBundle allSettings, ILog log)
        {
            BackOfficeServiceSettings settings = allSettings.PayBackOffice.Service;
            
            container.RegisterClientSessionService(settings.SessionUrl, log);
            container.RegisterAntifraudDetectorClient(settings.AntifraudDetectorUrl, log);
            container.RegisterBalancesClient(allSettings.BalancesServiceClient.ServiceUrl, log);
        }
    }
}
