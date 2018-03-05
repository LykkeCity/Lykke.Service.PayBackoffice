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
            IReloadingManager<SmsNotificationsSettings> smsNotificationsSettigns,
            IReloadingManager<string> pdfGeneratorConnectionString,
            double defaultWithdrawalLimit,
            ICacheManager cacheManager,
            ILog log)
        {
            MapEntities();

            container.RegisterInstance<IEthererumPendingActionsRepository>(
              new EthererumPendingActionsRepository(
                  AzureTableStorage<EthererumPendingActionEntity>.Create(
                      dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString), "EthererumPendingActions", log)));

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

            container.RegisterInstance<IClientBalanceChangeLogRepository>(
                AzureRepoFactories.Clients.CreateBalanceChangeLogRepository(dbSettings.ConnectionString(x => x.ClientBalanceLogsConnString),
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

            container.RegisterInstance<IBitCoinTransactionsRepository>(
                new BitCoinTransactionsRepository(
                    AzureTableStorage<BitCoinTransactionEntity>.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString),
                        "BitCoinTransactions", log)));

            container.RegisterInstance(new BitcoinTransactionContextBlobStorage(AzureBlobStorage.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString))))
                .As<IBitcoinTransactionContextBlobStorage>();

            container.RegisterInstance<IBackupQrRepository>(
                new BackupQrRepository(AzureBlobStorage.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString))));

            container.RegisterInstance<IAttachmentFileRepository>(new AttachmentFileRepository(
                AzureBlobStorage.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString))));

            container.RegisterInstance<IFeeOutputsStatusRepository>(new FeeOutputsStatusRepository(
                AzureBlobStorage.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString))));

            container.RegisterInstance<IAssetPairDetailedRatesRepository>(new AssetPairDetailedRatesRepository(
                AzureBlobStorage.Create(dbSettings.ConnectionString(x => x.HLiquidityConnString)), cacheManager));

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

            container.RegisterInstance<IMarketDataRepository>
                (new MarketDataRepository(
                    AzureTableStorage<MarketDataEntity>.Create(dbSettings.ConnectionString(x => x.HTradesConnString), "MarketsData", log)));

            container.RegisterInstance<IRouterCommandProducer>
                (new RouterCommandProducer(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.SharedStorageConnString), "router-income-queue")));

            container.RegisterInstance<ISrvSolarCoinCommandProducer>(
                new SrvSolarCoinCommandProducer(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.SolarCoinConnString), "solar-out")));
                        
            container.RegisterInstance<IRegulatorRepository>
            (new RegulatorRepository(
                AzureTableStorage<RegulatorEntity>.Create(dbSettings.ConnectionString(x => x.SharedStorageConnString), "Residences", log)));

            container.RegisterInstance<ISwiftCredentialsRepository>
            (new SwiftCredentialsRepository(
                AzureTableStorage<SwiftCredentialsEntity>.Create(dbSettings.ConnectionString(x => x.DictsConnString), "SwiftCredentials", log)));

            container.RegisterInstance<IPartnerAccountPolicyRepository>
            (new PartnerAccountPolicyRepository(
                AzureTableStorage<PartnerAccountPolicyEntity>.Create(dbSettings.ConnectionString(x => x.SharedStorageConnString), "PartnerAccountPolicy", log)));

            container.RegisterInstance<IPartnerClientAccountRepository>
            (new PartnerClientAccountRepository(
                AzureTableStorage<PartnerClientAccountEntity>.Create(dbSettings.ConnectionString(x => x.SharedStorageConnString), "PartnerClientAccounts", log)));

            container.RegisterInstance<IBitcoinCommandSender>(
                new BitcoinCommandSender(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString), "intransactions")));

            container.RegisterInstance<IClientDictionaryRepository>(
                new ClientDictionaryRepository(
                    AzureTableStorage<Clients.KeyValueEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "ClientsDictionaries", log)));

            container.RegisterInstance<IDictionaryRepository>(
                new DictionaryRepository(
                    AzureTableStorage<Application.KeyValueEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "AppDictionaries", log)));

            container.RegisterInstance<IDailyBalancesRepository>(
                new DailyBalancesRepository(
                    AzureTableStorage<DailyBalances>.Create(dbSettings.ConnectionString(x => x.OlapConnString), "DailyBalances", log)));

            container.RegisterInstance<IChronoBankCommandProducer>(
                new SrvChronoBankCommandProducer(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.ChronoBankSrvConnString), "chronobank-out")));

            container.RegisterInstance<IForwardWithdrawalRepository>(
                new ForwardWithdrawalRepository(
                    AzureTableStorage<ForwardWithdrawalEntity>.Create(dbSettings.ConnectionString(x => x.BalancesInfoConnString),
                        "ForwardWithdrawal", log)));

            container.RegisterInstance<IQuantaCommandProducer>(
                new SrvQuantaCommandProducer(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.QuantaSrvConnString), "quanta-out")));

            container.RegisterInstance<IUnsignedTransactionsRepository>(
                new UnsignedTransactionsRepository(
                    AzureTableStorage<UnsignedTransactionEntity>.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString),
                        "UnsignedTransactions", log)));

            container.RegisterInstance<ISignedMultisigTransactionsSender>(
                new SignedMultisigTransactionsSender(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.ClientSignatureConnString), "client-signed-transactions")));

            container.RegisterInstance<ISlackNotificationsProducer>(
                new SlackNotificationsProducer(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.SharedStorageConnString), "slack-notifications")));

            container.RegisterInstance<IInternalOperationsRepository>(new InternalOperationsRepository(
                AzureTableStorage<InternalOperationEntity>.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString),
                    "InternalOperations", log)));

            container.RegisterInstance<ILastProcessedBlockRepository>(new LastProcessedBlockRepository(
                AzureTableStorage<LastProcessedBlockEntity>.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString),
                    "LastProcessedBlocks", log)));

            container.RegisterInstance<IBalanceChangeTransactionsRepository>(new BalanceChangeTransactionsRepository(
                AzureTableStorage<BalanceChangeTransactionEntity>.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString),
                    "BalanceChangeTransactions", log)));

            container.RegisterInstance<IConfirmPendingTxsQueue>(
                new ConfirmPendingTxsQueue(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString),
                    "txs-confirm-pending")));

            container.RegisterInstance<IConfirmedTransactionsRepository>(
                new ConfirmedTransactionsRepository(AzureTableStorage<ConfirmedTransactionRecord>.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString),
                        "ConfirmedTransactions", log)));

            container.RegisterInstance<IBcnClientCredentialsRepository>(
                new BcnClientCredentialsRepository(
                    AzureTableStorage<BcnCredentialsRecordEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "BcnClientCredentials", log)));

            container.RegisterInstance<IEthereumTransactionRequestRepository>(
                new EthereumTransactionRequestRepository(
                    AzureTableStorage<EthereumTransactionReqEntity>.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString),
                        "EthereumTxRequest", log)));

            container.RegisterInstance<IEthClientEventLogs>(
                new EthClientEventLogs(
                    AzureTableStorage<EthClientEventRecord>.Create(dbSettings.ConnectionString(x => x.LwEthLogsConnString),
                        "EthClientEventLogs", log)));

            container.RegisterInstance<IAccessTokensRepository>(
                new AccessTokensRepository(
                    AzureTableStorage<AccessTokenRecord>.Create(dbSettings.ConnectionString(x => x.SecurityEventsConnString),
                        "AccessTokens", log)));

            container.RegisterInstance<ICashoutTemplateRepository>(
                new CashoutTemplateRepository(
                    AzureTableStorage<CashoutTemplateItem>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "CashoutTemplates", log)));

            container.RegisterInstance<ICashoutTemplateLogRepository>(
                new CashoutTemplateLogRepository(
                    AzureTableStorage<CashoutTemplateLogRecord>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "CashoutLogs", log)));

            container.RegisterInstance<IWithdrawLimitsRepository>(
                new WithdrawLimitsRepository(
                    AzureTableStorage<WithdrawLimitRecord>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "WithdrawLimits", log),
                    defaultWithdrawalLimit));

            container.RegisterInstance<IClientDialogsRepository>(
                new ClientDialogsRepository(
                    AzureTableStorage<ClientDialogEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "ClientDialogs",
                        log),
                    AzureTableStorage<TestSubmit>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "ClientDialogSubmitStub",
                        log)));

            container.RegisterInstance<IInternalTransferRepository>(new InternalTransferRepository(
                AzureTableStorage<InternalTransferEntity>.Create(dbSettings.ConnectionString(x => x.InternalTransactionsConnectionString),
                    "InternalTransferInfo", log)));

            if (!string.IsNullOrWhiteSpace(pdfGeneratorConnectionString?.CurrentValue))
            {
                container.RegisterInstance<IPdfGeneratorRepository>(new PdfGeneratorRepository(AzureBlobStorage.Create(pdfGeneratorConnectionString), log));
            }

            container.RegisterInstance<ISkipKycRepository>(
                new SkipKycRepository(AzureTableStorage<SkipKycClientEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "SkipKycClients", log)));

            container.RegisterInstance<IClientWithdrawalCacheRepository>(
                new ClientWithdrawalCacheRepository(
                    AzureTableStorage<ClientWithdrawalItem>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "WithdrawalCache", log)));

            container.BindSignatureRequests(dbSettings, log);
            container.BindTradingRepos(dbSettings, log);            
            container.BindBitCoinRepos(dbSettings, log);
            container.BindEventLog(dbSettings, log);
            container.BindPersonalInfoAndOther(dbSettings, log);
            container.BindEmailMessagesRepos(dbSettings, log);
            container.BindSmsMessagesRepos(smsNotificationsSettigns, log);
            container.BindCashTansferRepos(dbSettings, log);
            container.BindOffchain(dbSettings, log);
        }

        private static void BindSignatureRequests(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterInstance<ISignatureRequestRepository>(
             new SignatureRequestRepository(AzureTableStorage<SignatureRequestEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                   "SignRequests", log)));

            container.RegisterInstance<ISignatureCommandProducer>
                (new SignatureCommandProducer(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.SharedStorageConnString), "router-signed-request-queue")));
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

            container.RegisterInstance<Core.MarginTrading.Repositories.IMaintenanceInfoRepository>(
                new MaintenanceInfoRepository(
                    AzureTableStorage<MaintenanceInfoEntity>.Create(dbSettings.ConnectionString(x => x.MarginTradingFrontendConnString), "MaintenanceInfo", log)));
        }

        private static void BindRequestsLog(ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            var storage = AzureTableStorage<RequestsLogRecord>.Create(dbSettings.ConnectionString(x => x.LogsConnString), "ApiRequests", log);
            var repository = new RequestsLogRepository(storage, new RequestsLogPersistenceManager(storage, log), log);

            repository.Start();

            container.RegisterInstance<IRequestsLogRepository>(repository);
        }
        
        private static void BindTradingRepos(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterInstance<IClientTradesRepository>(
                new ClientTradesRepository(AzureTableStorage<ClientTradeEntity>.Create(dbSettings.ConnectionString(x => x.HTradesConnString), "Trades", log)));

            container.RegisterInstance<IWalletsRepository>(
                AzureRepoFactories.Wallets.CreateAccountsRepository(dbSettings.ConnectionString(x => x.BalancesInfoConnString), log));

            container.RegisterInstance<IBalancePendingRepository>(
                new BalancePendingRepository(AzureTableStorage<BalancePendingEntity>.Create(dbSettings.ConnectionString(x => x.BalancesInfoConnString),
                    "BalancePending", log)));

            container.RegisterInstance<IMarketOrdersRepository>(
                new MarketOrdersRepository(AzureTableStorage<MarketOrderEntity>.Create(dbSettings.ConnectionString(x => x.HMarketOrdersConnString),
                    "MarketOrders", log)));

            container.RegisterInstance<ILimitOrdersRepository>(
                new LimitOrdersRepository(AzureTableStorage<LimitOrderEntity>.Create(dbSettings.ConnectionString(x => x.HMarketOrdersConnString),
                    "LimitOrders", log)));

            container.RegisterInstance<IOrderTradesLinkRepository>(
                new OrderTradesLinkRepository(AzureTableStorage<OrderTradeLinkEntity>.Create(dbSettings.ConnectionString(x => x.HMarketOrdersConnString),
                    "OrderTradesLinks", log)));

            container.RegisterInstance<ICoastlineTraderStateRepository>(
                new CoastlineTraderStateRepository(
                    AzureTableStorage<CoastlineTraderStateEntity>.Create(dbSettings.ConnectionString(x => x.AlphaEngineAuditConnString), "CoastlineTradersStates", log),
                    AzureTableStorage<CoastlineTraderOrderEntity>.Create(dbSettings.ConnectionString(x => x.AlphaEngineAuditConnString), "CoastlineTradersOrders", log)));

            container.RegisterInstance<ICoastlineTraderOperationRepository>(
                new CoastlineTraderOperationRepository(AzureTableStorage<CoastlineTraderOperationDataEntity>.Create(dbSettings.ConnectionString(x => x.AlphaEngineAuditConnString),
                    "CoastlineTradersOperations", log)));

            container.RegisterInstance<IExchangeStateRepository>(
                new ExchangeStateRepository(AzureTableStorage<ExchangeStateDataEntity>.Create(dbSettings.ConnectionString(x => x.AlphaEngineAuditConnString),
                    "ExchangesStates", log)));

            container.RegisterInstance<IExchangeOperationRepository>(
                new ExchangeOperationRepository(AzureTableStorage<ExchangeOperationDataEntity>.Create(dbSettings.ConnectionString(x => x.AlphaEngineAuditConnString),
                    "ExchangesOperations", log)));
        }

        public static void BindBitCoinRepos(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterInstance<IBlockchainTransactionsCache>(
                AzureRepoFactories.Bitcoin.CreateBlockchainTransactionsCache(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString),
                    log)
                );
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

        public static void BindCashTansferRepos(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterInstance<ICashOutAttemptRepository>(
                new CashOutAttemptRepository(
                    AzureTableStorage<CashOutAttemptEntity>.Create(dbSettings.ConnectionString(x => x.BalancesInfoConnString), "CashOutAttempt", log)));

            container.RegisterInstance<ICashoutRequestLogRepository>(
                new CashoutRequestLogRepository(
                    AzureTableStorage<CashoutRequestLogRecord>.Create(dbSettings.ConnectionString(x => x.BalancesInfoConnString), "CashOutAttemptLog", log)));

            container.RegisterInstance<ICashoutsProcessedReportRepository>(
                new CashoutsProcessedReportRepository(
                    AzureTableStorage<CashoutProcessedReportRowEntity>.Create(dbSettings.ConnectionString(x => x.BalancesInfoConnString), "CashoutsProcessedReport", log)));

            container.RegisterInstance<ICashoutPaymentDateRepository>(
                new CashOperations.CashoutPaymentDateRepository(
                    AzureTableStorage<CashoutPaymentDateEntity>.Create(dbSettings.ConnectionString(x => x.BalancesInfoConnString), "CashoutPaymentDates", log)));
        }

        public static void BindOffchain(this ContainerBuilder container, IReloadingManager<DbSettings> dbSettings, ILog log)
        {
            container.RegisterInstance<IOffchainRequestRepository>(
                new OffchainRequestRepository(
                    AzureTableStorage<OffchainRequestEntity>.Create(dbSettings.ConnectionString(x => x.OffchainConnString), "OffchainRequests", log)));

            container.RegisterInstance<IOffchainTransferRepository>(
                new OffchainTransferRepository(
                    AzureTableStorage<OffchainTransferEntity>.Create(dbSettings.ConnectionString(x => x.OffchainConnString), "OffchainTransfers", log)));

            container.RegisterInstance<IOffchainOrdersRepository>(
                new OffchainOrderRepository(
                    AzureTableStorage<OffchainOrder>.Create(dbSettings.ConnectionString(x => x.OffchainConnString), "OffchainOrders", log)));

            container.RegisterInstance<IOffchainEncryptedKeysRepository>(
                new OffchainEncryptedKeyRepository(
                    AzureTableStorage<OffchainEncryptedKeyEntity>.Create(dbSettings.ConnectionString(x => x.OffchainConnString), "OffchainEncryptedKeys", log)));

            container.RegisterInstance<IOffchainSettingsRepository>(
                new OffchainSettingsRepository(
                    AzureTableStorage<OffchainSettingEntity>.Create(dbSettings.ConnectionString(x => x.OffchainConnString), "OffchainSettings", log)));

            container.RegisterInstance<IOffchainFinalizeCommandProducer>(new OffchainFinalizeCommandProducer(AzureQueueExt.Create(dbSettings.ConnectionString(x => x.BitCoinQueueConnectionString), "offchain-finalization")));
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
