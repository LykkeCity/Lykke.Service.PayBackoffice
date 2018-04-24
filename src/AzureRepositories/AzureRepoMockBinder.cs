using Autofac;
using AzureRepositories.Accounts;
using AzureRepositories.Accounts.PrivateWallets;
using AzureRepositories.BackOffice;
using AzureRepositories.Bitcoin;
using AzureRepositories.CashOperations;
using AzureRepositories.Clients;
using AzureRepositories.Exchange;
using AzureRepositories.Log;
using AzureRepositories.PaymentSystems;
using AzureRepositories.Users;
using AzureRepositories.VerificationCode;
using AzureStorage.Blob;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Core;
using Core.Accounts;
using Core.Accounts.PrivateWallets;
using Core.BackOffice;
using Core.BitCoin;
using Core.CashOperations;
using Core.Clients;
using Core.Exchange;
using Core.PaymentSystems;
using Core.Users;
using Core.VerificationCode;
using Lykke.Messages.Email;

namespace AzureRepositories
{
    public static class AzureRepoMockBinder
    {

        public static void BindAzureReposInMem(this ContainerBuilder ioc)
        {
            var localHost = @"http://127.0.0.1:8998";
            
           ioc.RegisterInstance<IIdentityGenerator>(
                new IdentityGenerator(new AzureTableStorageLocal<IdentityEntity>(localHost, "IdentityGenerator")));

           ioc.RegisterInstance<IBrowserSessionsRepository>(
                new BrowserSessionsRepository(new AzureTableStorageLocal<BrowserSessionEntity>(localHost, "BrowserSessionsRepository")));

           ioc.RegisterInstance<IMenuBadgesRepository>(
                new MenuBadgesRepository(new AzureTableStorageLocal<MenuBadgeEntity>(localHost, "MenuBadgesRepository")));

           ioc.RegisterInstance<IPinSecurityRepository>(
                new PinSecurityRepository(new AzureTableStorageLocal<PinSecurityEntity>(localHost, "ClientPins")));

           ioc.RegisterInstance<IClientLog>(
                new ClientLog(new AzureTableStorageLocal<ClientLogItem>(localHost, "LogClient")));


           ioc.RegisterInstance<IWalletCredentialsRepository>(
                new WalletCredentialsRepository(new AzureTableStorageLocal<WalletCredentialsEntity>(localHost, "WalletCredentials")));

           ioc.RegisterInstance<IExchangeSettingsRepository>(
                new ExchangeSettingsRepository(new AzureTableStorageLocal<ExchangeSettingsEntity>(localHost,
                    "ExchangeSettings")));

           ioc.RegisterInstance<IBitCoinTransactionsRepository>(
                new BitCoinTransactionsRepository(new AzureTableStorageLocal<BitCoinTransactionEntity>(localHost, "BitCoinTransactions")));

           ioc.RegisterInstance<IBackOfficeUsersRepository>(
               new BackOfficeUsersRepository(new AzureTableStorageLocal<BackOfficeUserEntity>(localHost, "BackOfficeUsers")));

           ioc.RegisterInstance<IBackofficeUserRolesRepository>(
                new BackOfficeUserRolesRepository(new NoSqlTableInMemory<BackofficeUserRoleEntity>()));

           ioc.RegisterInstance<IMarketOrdersRepository>(
                new MarketOrdersRepository(new AzureTableStorageLocal<MarketOrderEntity>(localHost, "MarketOrders")));

           ioc.RegisterInstance<ICashOperationsRepository>(
                new CashOperationsRepository(new AzureTableStorageLocal<CashInOutOperationEntity>(localHost, "OperationsCash"),
                new AzureTableStorageLocal<AzureIndex>(localHost, "OperationsCash")));

           ioc.RegisterInstance<IUserSettingsRepository>(
                new UserSettingsRepository(new AzureTableStorageLocal<UserSettingsEntity>(localHost, "UserSettings")));

           ioc.RegisterInstance<IClientTradesRepository>(
                new ClientTradesRepository(new AzureTableStorageLocal<ClientTradeEntity>(localHost, "Trades")));
        }

        public static void BindAzureReposInMemForTests(this ContainerBuilder ioc)
        {
            ioc.RegisterEmailSenderViaInmemoryQueueMessageProducer();

           ioc.RegisterInstance<IBalancePendingRepository>(
                new BalancePendingRepository(
                    new NoSqlTableInMemory<BalancePendingEntity>()));
            
           ioc.RegisterInstance<IFeeOutputsStatusRepository>(
                new FeeOutputsStatusRepository(new AzureBlobInMemory()));

           ioc.RegisterInstance<IIdentityGenerator>(
                new IdentityGenerator(new NoSqlTableInMemory<IdentityEntity>()));
            
           ioc.RegisterInstance<IBrowserSessionsRepository>(
                new BrowserSessionsRepository(new NoSqlTableInMemory<BrowserSessionEntity>()));

           ioc.RegisterInstance<IMenuBadgesRepository>(
                new MenuBadgesRepository(new NoSqlTableInMemory<MenuBadgeEntity>()));

           ioc.RegisterInstance<IPinSecurityRepository>(
                new PinSecurityRepository(new NoSqlTableInMemory<PinSecurityEntity>()));

           ioc.RegisterInstance<IMarketOrdersRepository>(
                new MarketOrdersRepository(new NoSqlTableInMemory<MarketOrderEntity>()));

           ioc.RegisterInstance<IExchangeSettingsRepository>(new ExchangeSettingsRepository(
                new NoSqlTableInMemory<ExchangeSettingsEntity>()));

           ioc.RegisterInstance<ICashOperationsRepository>(
                new CashOperationsRepository(new NoSqlTableInMemory<CashInOutOperationEntity>(),
                    new NoSqlTableInMemory<AzureIndex>()));

           ioc.RegisterInstance<IClientTradesRepository>(
                new ClientTradesRepository(new NoSqlTableInMemory<ClientTradeEntity>()));

           ioc.RegisterInstance<IBitCoinTransactionsRepository>(
                new BitCoinTransactionsRepository(new NoSqlTableInMemory<BitCoinTransactionEntity>()));

           ioc.RegisterInstance<IWalletCredentialsRepository>(
                new WalletCredentialsRepository(new NoSqlTableInMemory<WalletCredentialsEntity>()));

           ioc.RegisterInstance<ITransferEventsRepository>(
                new TransferEventsRepository(new NoSqlTableInMemory<TransferEventEntity>(),
                new NoSqlTableInMemory<AzureIndex>()));

           ioc.RegisterInstance<IEmailVerificationCodeRepository>(
                new EmailVerificationCodeRepository(
                    new NoSqlTableInMemory<EmailVerificationCodeEntity>(),
                    new NoSqlTableInMemory<EmailVerificationPriorityCodeEntity>()));

           ioc.RegisterInstance<IPrivateWalletsRepository>(
                new PrivateWalletsRepository(new NoSqlTableInMemory<PrivateWalletEntity>()));

           ioc.RegisterInstance<IPaymentTransactionsRepository>(
                new PaymentTransactionsRepository(new NoSqlTableInMemory<PaymentTransactionEntity>(), new NoSqlTableInMemory<AzureMultiIndex>()));

           ioc.RegisterInstance<IPaymentTransactionEventsLog>(
                new PaymentTransactionEventsLog(new NoSqlTableInMemory<PaymentTransactionLogEventEntity>()));

           ioc.RegisterInstance<IWalletCredentialsHistoryRepository>(
                new WalletCredentialsHistoryRepository(new NoSqlTableInMemory<WalletCredentialsHistoryRecord>()));
        }
    }
}
