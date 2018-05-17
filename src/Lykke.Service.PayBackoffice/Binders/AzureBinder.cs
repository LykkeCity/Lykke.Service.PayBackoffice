﻿using Autofac;
using BackOffice.Settings;
using Common.Cache;
using Common.IocContainer;
using Lykke.Service.BackofficeMembership.Client;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.Configuration;
using QBitNinja.Client;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.MonitoringServiceApiCaller;

namespace BackOffice.Binders
{
    public class AzureBinder : IDependencyBinder
    {
        internal static string BlockchainExplorerUrl;
        internal static string PayInvoicePortalResetPasswordLink;
        private string _monitoringServiceUrl;
        public ContainerBuilder Bind(IConfigurationRoot configuration, ContainerBuilder builder = null)
        {
            return Bind(configuration, builder, false);
        }

        public ContainerBuilder Bind(IConfigurationRoot configuration, ContainerBuilder builder = null, 
            bool fromTest = false)
        {
            var settings = configuration.LoadSettings<BackOfficeBundle>();
            _monitoringServiceUrl = settings.CurrentValue.MonitoringServiceClient?.MonitoringServiceUrl;
            BlockchainExplorerUrl = settings.CurrentValue.PayBackOffice.BlockchainExplorerUrl;
            PayInvoicePortalResetPasswordLink = settings.CurrentValue.PayBackOffice.PayInvoicePortalResetPasswordLink;
            var ioc = builder ?? new ContainerBuilder();
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice);
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.GoogleAuthSettings);
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.TwoFactorVerification ?? new TwoFactorVerificationSettingsEx());
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.LykkePayWalletList);

            var cacheManager = new MemoryCacheManager();
            ioc.RegisterInstance<ICacheManager>(cacheManager);

            ioc.RegisterInstance<IEmailPartnerRouterClient>(new EmailPartnerRouterClient(settings.CurrentValue.EmailPartnerRouterServiceClient.ServiceUrl));
            ioc.RegisterInstance<IPayInternalClient>(new PayInternalClient(new PayInternalServiceClientSettings() { ServiceUrl = settings.CurrentValue.PayInternalServiceClient.ServiceUrl }));
            ioc.RegisterInstance<IPayInvoiceClient>(new PayInvoiceClient(new PayInvoiceServiceClientSettings() { ServiceUrl = settings.CurrentValue.PayInvoiceServiceClient.ServiceUrl }));
            ioc.RegisterInstance<IPayAuthClient>(new PayAuthClient(new PayAuthServiceClientSettings() { ServiceUrl = settings.CurrentValue.PayAuthServiceClient.ServiceUrl }));

            ioc.RegisterInstance(new QBitNinjaClient(settings.CurrentValue.NinjaServiceClient.ServiceUrl)).AsSelf();

            ioc.RegisterBackofficeMembershipClient(settings.CurrentValue.BackofficeMembershipServiceClient.ServiceUrl);

#if !DEBUG
                if (!string.IsNullOrEmpty(_monitoringServiceUrl))
                    await AutoRegistrationInMonitoring.RegisterAsync(Configuration, _monitoringServiceUrl, Log);
#endif
            return ioc;
        }
    }
}
