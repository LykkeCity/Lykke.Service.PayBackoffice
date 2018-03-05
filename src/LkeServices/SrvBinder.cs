using System;
using System.Linq;
using System.Net;
using Autofac;
using Common;
using Common.Log;
using Core;
using Core.BitCoin;
using Core.LykkeServiceApi;
using Core.Messages;
using Core.Settings;
using LkeServices.Bitcoin;
using LkeServices.Kyc;
using LkeServices.LykkeServiceApi;
using LkeServices.Messages.Email;
using LkeServices.PaymentSystems;
using LkeServices.PaymentSystems.PaymentOkNotificators;
using LkeServices.Pdf;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.MonitoringServiceApiCaller;
using LkeServices.Export;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.GoogleDriveUpload.Client;
using Lykke.Service.PersonalData.Settings;

namespace LkeServices
{
    public static class SrvBinder
    {
        public static void BindLykkeServicesApi(this ContainerBuilder ioc, LykkeServiceApiSettings serviceApiSettings)
        {
            ioc.Register<ILykkeServiceApiConnector>(x => new LykkeServiceApiConnector(serviceApiSettings));
            
            ioc.RegisterType<AssetApiService>().As<IAssetApiService>().SingleInstance();
            ioc.RegisterType<AssetPairApiService>().As<IAssetPairApiService>().SingleInstance();
        }
        
        public static void BindCachedDicts(this ContainerBuilder ioc)
        {
            ioc.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedAssetsDictionary
                (
                    async () => (await assetsService.AssetGetAllAsync(includeNonTradable: true)).ToDictionary(itm => itm.Id)
                );

            }).SingleInstance();

            ioc.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedTradableAssetsDictionary
                (
                    async () => (await assetsService.AssetGetAllAsync(includeNonTradable: false)).ToDictionary(itm => itm.Id)
                );

            }).SingleInstance();
            
            ioc.Register(x =>
            {

                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedDataDictionary<string, AssetPair>
                (
                    async () => (await assetsService.AssetPairGetAllAsync()).ToDictionary(itm => itm.Id)
                );

            }).SingleInstance();
        }

        #region MatchingEngine

        public static void BindMatchingEngineChannel(this ContainerBuilder container, IPEndPoint ipEndPoint)
        {
            var socketLog = new SocketLogDynamic(i => { },
                str => Console.WriteLine(DateTime.UtcNow.ToIsoDateTime() + ": " + str));

            container.BindMeClient(ipEndPoint, socketLog);
        }

        public static void BindMatchingEngineServices(this ContainerBuilder ioc)
        {
            ioc.BindCachedDicts();
        }

        public static void StartMatchingEngineServices(this ContainerBuilder ioc)
        {

        }

        #endregion

        public static void RegisterClientServices(this ContainerBuilder ioc, PersonalDataServiceClientSettings settings)
        {
            ioc.RegisterType<PersonalDataService>().
                As<IPersonalDataService>()
                .WithParameter(TypedParameter.From(settings));
        }

        public static void RegisterAllServices(this ContainerBuilder ioc, SupportToolsSettings supportToolsSettings)
        {
            // Settings
            ioc.RegisterInstance(supportToolsSettings);

            // Payment services
            ioc.RegisterType<PaymentOkEmailSender>().As<IPaymentOkNotification>().SingleInstance();

            // Email Services
            ioc.RegisterType<SrvEmailsFacade>().As<ISrvEmailsFacade>().SingleInstance();
            
            ioc.RegisterType<ExportService>().SingleInstance();
            ioc.RegisterType<SrvPdfGenerator>().SingleInstance();

            ioc.RegisterType<BitcoinTransactionService>().As<IBitcoinTransactionService>().SingleInstance();
            ioc.RegisterType<JumioService>().As<IJumioService>().SingleInstance();
        }

        public static void RegisterMonitoringServices(this ContainerBuilder ioc, string monitoringServiceUrl)
        {
            //MonitoringService
            ioc.RegisterInstance<MonitoringServiceFacade>(new MonitoringServiceFacade(monitoringServiceUrl));
        }

        public static void RegisterGoogleDriveUploadService(this ContainerBuilder ioc, string serviceUrl, ILog log)
        {
            ioc.RegisterInstance<IGoogleDriveUploadClient>(new GoogleDriveUploadClient(serviceUrl, log));
        }
    }
}
