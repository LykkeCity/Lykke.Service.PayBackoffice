using Autofac;
using AutoMapper;
using AzureRepositories.Accounts;
using AzureRepositories.Accounts.PrivateWallets;
using AzureRepositories.Accounts.Recovery;
using AzureRepositories.Application;
using AzureRepositories.Assets;
using AzureRepositories.AuditLog;
using AzureRepositories.BackgroundWorker;
using AzureRepositories.BackOffice;
using AzureRepositories.Bitcoin;
using AzureRepositories.Blockchain;
using AzureRepositories.Blockchain.Router;
using AzureRepositories.Blockchain.Signature;
using AzureRepositories.Broadcast;
using AzureRepositories.CashOperations;
using AzureRepositories.CashTransfers;
using AzureRepositories.ChronoBank;
using AzureRepositories.Clients;
using AzureRepositories.Email;
using AzureRepositories.Ethereum;
using AzureRepositories.EventLogs;
using AzureRepositories.Exchange;
using AzureRepositories.Iata;
using AzureRepositories.InternalTransfers;
using AzureRepositories.Log;
using AzureRepositories.MarginTrading;
using AzureRepositories.Monitoring;
using AzureRepositories.Notifications;
using AzureRepositories.Offchain;
using AzureRepositories.Partners;
using AzureRepositories.PaymentSystems;
using AzureRepositories.Quanta;
using AzureRepositories.RemoteUi;
using AzureRepositories.Settings;
using AzureRepositories.Sms;
using AzureRepositories.SolarCoin;
using AzureRepositories.Regulator;
using AzureRepositories.Security;
using AzureRepositories.SwiftCredentials;
using AzureRepositories.Users;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Cache;
using Common.Log;
using Core;
using Core.Accounts;
using Core.Accounts.PrivateWallets;
using Core.Accounts.Recovery;
using Core.Application;
using Core.Assets;
using Core.AuditLog;
using Core.BackgroundWorker;
using Core.BackOffice;
using Core.Bitcoin;
using Core.BitCoin;
using Core.BitCoin.Ninja;
using Core.Blockchain;
using Core.Blockchain.Router;
using Core.Blockchain.Signature;
using Core.Broadcast;
using Core.CashOperations;
using Core.ChronoBank;
using Core.Clients;
using Core.Ethereum;
using Core.EventLogs;
using Core.Exchange;
using Core.Iata;
using Core.MarginTrading.Payments;
using Core.Messages.Email;
using Core.Messages.Sms;
using Core.Monitoring;
using Core.Notifications;
using Core.Offchain;
using Core.Partner;
using Core.PaymentSystems;
using Core.Regulator;
using Core.Quanta;
using Core.RemoteUi;
using Core.Security;
using Core.Settings;
using Core.SolarCoin;
using Core.SwiftCredentials;
using Core.Users;
using Core.VerificationCode;
using Microsoft.WindowsAzure.Storage.Table;
using AzureRepositories.Pdf;
using Core.CoastlineTraders;
using Core.Pdf;
using Lykke.Logs;
using Lykke.Messages.Email;
using Lykke.SettingsReader;
using AzureRepositories.CoastlineTraders;
using Core.MmSettings;
using AzureRepositories.MmSettings;
using Core.InternalTransfers;

namespace AzureRepositories
{
    public static class AzureRepoBinder
    {
        public static void MapEntities()
        {
            //NOTE: USE ONLY ONE Mapper.Initialize Method call
            Mapper.Initialize(cfg => {

                cfg.CreateMap<IBcnCredentialsRecord, BcnCredentialsRecordEntity>().IgnoreTableEntityFields();

                cfg.CreateMap<IEthereumTransactionRequest, EthereumTransactionReqEntity>().IgnoreTableEntityFields()
                .ForMember(x => x.SignedTransferVal, config => config.Ignore())
                .ForMember(x => x.OperationIdsVal, config => config.Ignore());

                cfg.CreateMap<ICashoutProcessedReportRow, CashoutProcessedReportRowEntity>().IgnoreTableEntityFields()
                .ForMember(x => x.SwiftDataJson, config => config.Ignore());
            });

            Mapper.Configuration.AssertConfigurationIsValid();
        }

        public static void BindAzureRepositories(this ContainerBuilder container,
            IReloadingManager<DbSettings> dbSettings,
            ICacheManager cacheManager,
            ILog log)
        {
            MapEntities();

            container.RegisterInstance<IAddressBookEntityRepository>(
                AzureRepoFactories.Clients.CreateAddressBookEntityRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IAddressBookItemRepository>(
                AzureRepoFactories.Clients.CreateAddressBookItemRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IAddressBookPhoneNumbersWhiteListRepository>(
                AzureRepoFactories.Clients.CreateAddressBookPhoneNumbersWhiteListRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IPinSecurityRepository>(
                AzureRepoFactories.Clients.CreatePinSecurityRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IRecoveryTokensRepository>(
               AzureRepoFactories.Clients.CreateRecoveryTokensRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<ISmsVerificationCodeRepository>(
                AzureRepoFactories.VerificationCodes.CreateSmsVerificationRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IEmailVerificationCodeRepository>(
                AzureRepoFactories.VerificationCodes.CreateEmailVerificationRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IIdentityGenerator>(
                AzureRepoFactories.CreateIdentityGenerator(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IClientCacheRepository>(
                new ClientCacheRepository(AzureTableStorage<ClientCacheEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                    "ClientCache", log)));

            container.RegisterInstance<IBrowserSessionsRepository>(
                AzureRepoFactories.CreateBrowserSessionsRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IApplicationRepository>(
                AzureRepoFactories.Applications.CreateApplicationsRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                    log));

            container.RegisterInstance<ITemporaryIdRepository>(
                new TemporaryIdRepository(AzureTableStorage<TemporaryIdRecord>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "TemporaryClientLinks", log)));

            container.RegisterInstance<IWalletCredentialsRepository>(
                new WalletCredentialsRepository(
                    AzureTableStorage<WalletCredentialsEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "WalletCredentials", log)));

            container.RegisterInstance<IWalletsGenerationHistory>(
                new WalletsGenerationHistory(AzureTableStorage<WalletsGenerationRecord>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "WalletsGenerationHistory", log)));

            container.RegisterInstance<IWalletCredentialsHistoryRepository>(
                new WalletCredentialsHistoryRepository(
                    AzureTableStorage<WalletCredentialsHistoryRecord>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "WalletCredentialsHistory", log)));

            container.RegisterInstance<IPrivateWalletsRepository>(
                new PrivateWalletsRepository(AzureTableStorage<PrivateWalletEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "PrivateWallets", log)));

            container.RegisterInstance<IPrivateWalletBackupRepository>(
                new PrivateWalletBackupRepository(
                    AzureTableStorage<PrivateWalletBackupEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "PrivateWalletBackups",
                        log)));

            container.RegisterInstance<IExchangeSettingsRepository>(
                new ExchangeSettingsRepository(
                    AzureTableStorage<ExchangeSettingsEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "ExchangeSettings", log)));

            container.RegisterInstance<IBackupQrRepository>(
                new BackupQrRepository(AzureBlobStorage.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString))));

            container.RegisterInstance<IAttachmentFileRepository>(new AttachmentFileRepository(
                AzureBlobStorage.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString))));

            container.RegisterInstance<IEmailAttachmentsMockRepository>(
                new EmailAttachmentsMockRepository(
                    AzureTableStorage<EmailAttachmentsMockEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "EmailAttachmentsMock", log)));

            container.RegisterInstance<IAppGlobalSettingsRepositry>(
                new AppGlobalSettingsRepository(
                    AzureTableStorage<AppGlobalSettingsEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "Setup", log)));

            container.RegisterInstance<IAuditLogRepository>(
                new AuditLogRepository(
                    AzureTableStorage<AuditLogDataEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "AuditLogs", log)));

            container.RegisterInstance<IMmSettingsAuditLogRepository>(
                new MmSettingsAuditLogRepository(
                    AzureTableStorage<MmSettingsAuditLogDataEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "MmSettingsAuditLogs", log)));

            container.RegisterInstance<IPaymentTransactionsRepository>(
                AzureRepoFactories.CreateBankCardOrderRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IBackgroundWorkRequestProducer>(
                new BackgroundWorkRequestProducer(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "background-worker")));

            container.RegisterInstance<IPrivateKeyOwnershipMsgRepository>(
                new PrivateKeyOwnershipMsgRepository(
                    AzureTableStorage<PrivateKeyOwnershipMsgEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "PKOwnershipMsg", log)));

            container.RegisterInstance<IServiceMonitoringRepository>(
                new ServiceMonitoringRepository(
                    AzureTableStorage<MonitoringRecordEntity>.Create(dbSettings.ConnectionString(x => x.SharedStorageConnString), "Monitoring", log)));

            container.RegisterInstance<ICallTimeLimitsRepository>(
                new CallTimeLimitsRepository(
                    AzureTableStorage<ApiCallHistoryRecord>.Create(dbSettings.ConnectionString(x => x.LogsConnString), "ApiSuccessfulCalls", log)));

            container.RegisterInstance<ISmsProviderUsageLogs>(
                new SmsProviderUsageLogs(
                    AzureTableStorage<SmsProviderUsageRecordEntity>.Create(dbSettings.ConnectionString(x => x.LogsConnString), "SmsProviderUsageLogs", log)));

            container.RegisterInstance<IRequestVoiceCallRepository>(
                new RequestVoiceCallRepository(
                    AzureTableStorage<RequestVoiceCallRecordEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "VoiceCallRequests", log)));

            container.RegisterInstance<ITempDataRepository>(
                new TempDataRepository(
                    AzureTableStorage<TempDataRecordEntity>.Create(dbSettings.ConnectionString(x => x.LogsConnString), "TempData", log)));

            container.RegisterInstance<ILkkSourceWalletsRepository>(
                new LkkSourceWalletRepository(
                    AzureTableStorage<LkkSourceWalletEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "IcoLkkSourceWallets", log)));

            container.RegisterInstance<IBannedClientsRepository>(
                new BannedClientsRepository(cacheManager,
                    AzureTableStorage<BannedClientEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "BannedClients", log)));

            container.RegisterInstance<IClientCommentsRepository>(
                new ClientCommentsRepository(AzureTableStorage<ClientCommentEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "ClientComments", log)));

            container.RegisterInstance<IClientDictionaryRepository>(
                new ClientDictionaryRepository(
                    AzureTableStorage<Clients.KeyValueEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "ClientsDictionaries", log)));

            container.RegisterInstance<IDictionaryRepository>(
                new DictionaryRepository(
                    AzureTableStorage<Application.KeyValueEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "AppDictionaries", log)));

            container.RegisterInstance<ISignedMultisigTransactionsSender>(
                new SignedMultisigTransactionsSender(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.ClientSignatureConnString), "client-signed-transactions")));

            container.RegisterInstance<ICashoutTemplateRepository>(
                new CashoutTemplateRepository(
                    AzureTableStorage<CashoutTemplateItem>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "CashoutTemplates", log)));

            container.RegisterInstance<ICashoutTemplateLogRepository>(
                new CashoutTemplateLogRepository(
                    AzureTableStorage<CashoutTemplateLogRecord>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "CashoutLogs", log)));

            container.RegisterInstance<IClientDialogsRepository>(
                new ClientDialogsRepository(
                    AzureTableStorage<ClientDialogEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "ClientDialogs",
                        log),
                    AzureTableStorage<TestSubmit>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "ClientDialogSubmitStub",
                        log)));

            container.RegisterInstance<ISkipKycRepository>(
                new SkipKycRepository(AzureTableStorage<SkipKycClientEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "SkipKycClients", log)));

            container.RegisterInstance<IClientWithdrawalCacheRepository>(
                new ClientWithdrawalCacheRepository(
                    AzureTableStorage<ClientWithdrawalItem>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "WithdrawalCache", log)));

            container.BindSignatureRequests(dbSettings, log);
            container.BindEventLog(dbSettings, log);
            container.BindPersonalInfoAndOther(dbSettings, log);
            container.BindEmailMessagesRepos(dbSettings, log);
        }

        private static void BindSignatureRequests(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterInstance<ISignatureRequestRepository>(
             new SignatureRequestRepository(AzureTableStorage<SignatureRequestEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                   "SignRequests", log)));
        }

        private static void BindPersonalInfoAndOther(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterInstance<IClientAviaCompanyLinksRepository>(
                new ClientAviaCompanyLinksRepository(AzureTableStorage<ClientAviaCompanyLinkEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "AviaCompanyLinks", log)));
        }

        private static void BindEventLog(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterInstance<IPurchaseAttemptsLog>(AzureRepoFactories.EventLogs.CreatePurchaseLog(
                dbSettings.ConnectionString(x => x.LogsConnString), log));

            BindRequestsLog(container, dbSettings, log);

            container.RegisterInstance<ICashOperationsRepository>(
                new CashOperationsRepository(
                    AzureTableStorage<CashInOutOperationEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "OperationsCash", log),
                    AzureTableStorage<AzureIndex>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "OperationsCash", log))
            );

            container.RegisterInstance<ITransferEventsRepository>(
                new TransferEventsRepository(
                    AzureTableStorage<TransferEventEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "Transfers", log),
                    AzureTableStorage<AzureIndex>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "Transfers", log)));

            container.RegisterInstance<IPaymentTransactionEventsLog>(
                new PaymentTransactionEventsLog(
                    AzureTableStorage<PaymentTransactionLogEventEntity>.Create(dbSettings.ConnectionString(x => x.LogsConnString), "PaymentsLog", log))
            );

            container.RegisterInstance<IPaymentSystemsRawLog>(
                new PaymentSystemsRawLog(AzureTableStorage<PaymentSystemRawLogEventEntity>.Create(
                    dbSettings.ConnectionString(x => x.LogsConnString), "PaymentSystemsLog", log)));

            container.RegisterInstance<IMarginTradingPaymentLogRepository>(
                new MarginTradingPaymentLogRepository(
                    AzureTableStorage<MarginTradingPaymentLogEntity>.Create(dbSettings.ConnectionString(x => x.LogsConnString), "MarginTradingPaymentsLog", log)));

            container.RegisterInstance<Core.MarginTrading.IMarginTradingExternalExchangeRepository>(
                new MarginTradingExternalExchangeRepository(
                    AzureTableStorage<MarginTradingMarketMakerExchangeEntity>.Create(dbSettings.ConnectionString(x => x.BackOfficeConnString), "MarginTradingMarketMakerExternalExchange", log)));

            container.RegisterInstance<ILimitTradeEventsRepository>(
                new LimitTradeEventsRepository(
                    AzureTableStorage<LimitTradeEventEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "LimitTradeEvents", log)));
        }

        private static void BindRequestsLog(ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            var storage = AzureTableStorage<RequestsLogRecord>.Create(dbSettings.ConnectionString(x => x.LogsConnString), "ApiRequests", log);
            var repository = new RequestsLogRepository(storage, new RequestsLogPersistenceManager(storage, log), log);

            repository.Start();

            container.RegisterInstance<IRequestsLogRepository>(repository);
        }


        public static void BindBackOfficeRepositories(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterInstance<IMenuBadgesRepository>(
                AzureRepoFactories.BackOffice.CreateMenuBadgesRepository(dbSettings.ConnectionString(x => x.BackOfficeConnString), log));

            container.RegisterInstance<IBackOfficeUsersRepository>(
                new BackOfficeUsersRepository(
                    AzureTableStorage<BackOfficeUserEntity>.Create(dbSettings.ConnectionString(x => x.BackOfficeConnString), "BackOfficeUsers",
                        log)));


            container.RegisterInstance<IBackofficeUserRolesRepository>(
                new BackOfficeUserRolesRepository(
                    AzureTableStorage<BackofficeUserRoleEntity>.Create(dbSettings.ConnectionString(x => x.BackOfficeConnString), "Roles", log)));

            container.RegisterInstance<IUserSettingsRepository>(
                new UserSettingsRepository(AzureTableStorage<UserSettingsEntity>.Create(dbSettings.ConnectionString(x => x.BackOfficeConnString), "UserSettings", log)));

            container.RegisterInstance<IRemoteUiRepository>(
                new RemoteUiRepository(AzureTableStorage<RemoteUiEntity>.Create(dbSettings.ConnectionString(x => x.BackOfficeConnString), "Setup",
                    log)));
        }

        public static void BindEmailMessagesRepos(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterEmailSenderViaAzureQueueMessageProducer(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "emailsqueue");
            container.RegisterInstance<IBroadcastMailsRepository>(new BroadcastMailsRepository(
                AzureTableStorage<BroadcastMailEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "BroadcastMails", log)));
            container.RegisterInstance<IEmailMockRepository>(new EmailMockRepository(
                AzureTableStorage<EmailMockEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "MockMails", log)));
            container.RegisterInstance<IVerifiedEmailsRepository>(new VerifiedEmailsRepository(
                AzureTableStorage<VerifiedEmailEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "VerifiedEmails", log)));
        }

        public static void BindSmsMessagesRepos(this ContainerBuilder container, IReloadingManager<SmsNotificationsSettings> settings, ILog log)
        {
            container.RegisterInstance<ISmsCommandProducer>(
                new SmsCommandProducer(AzureQueueExt.Create(
                    settings.ConnectionString(x => x.AzureQueue.ConnectionString),
                    settings.CurrentValue.AzureQueue.QueueName)));

            container.RegisterInstance<ISmsMockRepository>(new SmsMockRepository(
                AzureTableStorage<SmsMessageMockEntity>.Create(
                    settings.ConnectionString(x => x.AzureQueue.ConnectionString),
                    "MockSms", log)));
        }

        public static ILog BindLog(this ContainerBuilder container, IReloadingManager<string> connectionString, string appName, string tableName)
        {
            var consoleLogger = new LogToConsole();

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                appName,
                AzureTableStorage<LogEntity>.Create(connectionString, tableName, consoleLogger),
                consoleLogger);

            var azureStorageLogger = new LykkeLogToAzureStorage(
                appName,
                persistenceManager,
                lastResortLog: consoleLogger,
                ownPersistenceManager: true);

            azureStorageLogger.Start();

            container.RegisterInstance<ILog>(azureStorageLogger);

            return azureStorageLogger;
        }

        public static void BindClientLog(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, string tableName)
        {
            container.RegisterInstance<IClientLog>(new ClientLog(AzureTableStorage<ClientLogItem>.Create(dbSettings.ConnectionString(x => x.LogsConnString), tableName, null)));
        }

        public static IMappingExpression<TSource, TDestination> IgnoreTableEntityFields<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> map) where TDestination : TableEntity
        {
            map.ForMember(x => x.ETag, config => config.Ignore());
            map.ForMember(x => x.PartitionKey, config => config.Ignore());
            map.ForMember(x => x.RowKey, config => config.Ignore());
            map.ForMember(x => x.Timestamp, config => config.Ignore());
            return map;
        }
    }

}
