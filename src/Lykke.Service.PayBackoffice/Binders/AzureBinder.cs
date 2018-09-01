using System.Collections.Generic;
using Autofac;
using AutoMapper;
using BackOffice.Cqrs.Projections;
using BackOffice.Settings;
using Common.Cache;
using Common.IocContainer;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Service.BackofficeMembership.Client;
using Lykke.Service.EmailPartnerRouter.Client;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayInvoice.Contract;
using Lykke.Service.PayInvoice.Contract.Commands;
using Lykke.Service.PayInvoice.Contract.Events;
using Lykke.Service.PayMerchant.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.Configuration;
using QBitNinja.Client;

namespace BackOffice.Binders
{
    public class AzureBinder : IDependencyBinder
    {
        internal static string BlockchainExplorerUrl;
        internal static string EthereumBlockchainExplorerUrl;
        internal static string PayInvoicePortalResetPasswordLink;

        private const string CommandsRoute = "commands";
        private const string EventsRoute = "events";

        public ContainerBuilder Bind(IConfigurationRoot configuration, ContainerBuilder builder = null)
        {
            var settings = configuration.LoadSettings<BackOfficeBundle>(options => {});

            BlockchainExplorerUrl = settings.CurrentValue.PayBackOffice.BlockchainExplorerUrl;
            EthereumBlockchainExplorerUrl = settings.CurrentValue.PayBackOffice.EthereumBlockchainExplorerUrl;
            PayInvoicePortalResetPasswordLink = settings.CurrentValue.PayBackOffice.PayInvoicePortalResetPasswordLink;

            var ioc = builder ?? new ContainerBuilder();

            ioc.RegisterInstance(new MapperProvider().GetMapper()).As<IMapper>();

            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice);
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.GoogleAuth);
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.TwoFactorVerification ??
                                 new BackOfficeTwoFactorVerificationSettings());
            ioc.RegisterInstance(settings.CurrentValue.PayBackOffice.LykkePayWalletList);

            var cacheManager = new MemoryCacheManager();
            ioc.RegisterInstance<ICacheManager>(cacheManager);

            ioc.RegisterInstance<IEmailPartnerRouterClient>(
                new EmailPartnerRouterClient(settings.CurrentValue.EmailPartnerRouterServiceClient.ServiceUrl));
            ioc.RegisterInstance<IPayInternalClient>(new PayInternalClient(new PayInternalServiceClientSettings()
                {ServiceUrl = settings.CurrentValue.PayInternalServiceClient.ServiceUrl}));
            ioc.RegisterInstance<IPayInvoiceClient>(new PayInvoiceClient(new PayInvoiceServiceClientSettings()
                {ServiceUrl = settings.CurrentValue.PayInvoiceServiceClient.ServiceUrl}));
            ioc.RegisterInstance<IPayAuthClient>(new PayAuthClient(new PayAuthServiceClientSettings()
                {ServiceUrl = settings.CurrentValue.PayAuthServiceClient.ServiceUrl}));
            ioc.RegisterPayMerchantClient(settings.CurrentValue.PayMerchantServiceClient, null);

            ioc.RegisterInstance(new QBitNinjaClient(settings.CurrentValue.NinjaServiceClient.ServiceUrl)).AsSelf();

            ioc.RegisterBackofficeMembershipClient(settings.CurrentValue.BackofficeMembershipServiceClient.ServiceUrl);

            RegisterCqrsEngine(settings, ioc);

            return ioc;
        }

        private void RegisterCqrsEngine(IReloadingManagerWithConfiguration<BackOfficeBundle> appSettings,
            ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context))
                .As<IDependencyResolver>()
                .SingleInstance();

            var rabbitSettings = new RabbitMQ.Client.ConnectionFactory
                {Uri = appSettings.CurrentValue.PayBackOffice.RabbitMq.SagasConnectionString};

            builder.RegisterType<EmployeeRegistrationErrorProjection>().SingleInstance();

            builder.Register(ctx =>
            {
                var logFactory = ctx.Resolve<ILogFactory>();
                return new MessagingEngine(
                    logFactory,
                    new TransportResolver(new Dictionary<string, TransportInfo>
                    {
                        {
                            "RabbitMq",
                            new TransportInfo(
                                rabbitSettings.Endpoint.ToString(),
                                rabbitSettings.UserName,
                                rabbitSettings.Password,
                                "None", "RabbitMq")
                        }
                    }),
                    new RabbitMqTransportFactory(logFactory));
            });

            builder.Register(ctx => new CqrsEngine(
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IDependencyResolver>(),
                    ctx.Resolve<MessagingEngine>(),
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                        "RabbitMq",
                        Lykke.Messaging.Serialization.SerializationFormat.ProtoBuf,
                        environment: "lykke")),

                    Register.BoundedContext(EmployeeRegistrationBoundedContext.Name)
                        .PublishingCommands(typeof(RegisterEmployeeCommand))
                        .To(EmployeeRegistrationBoundedContext.Name)
                        .With(CommandsRoute)
                        .ListeningEvents(typeof(EmployeeRegistrationFailedEvent), typeof(EmployeeUpdateFailedEvent), typeof(EmployeeRegisteredEvent), typeof(EmployeeUpdatedEvent))
                        .From(EmployeeRegistrationBoundedContext.Name)
                        .On(EventsRoute)
                        .WithProjection(typeof(EmployeeRegistrationErrorProjection), EmployeeRegistrationBoundedContext.Name)
                ))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }
    }
}
