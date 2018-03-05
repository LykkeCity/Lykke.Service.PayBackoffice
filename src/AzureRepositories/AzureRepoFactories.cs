using AzureRepositories.Accounts;
using AzureRepositories.Application;
using AzureRepositories.Assets;
using AzureRepositories.BackOffice;
using AzureRepositories.Bitcoin;
using AzureRepositories.Clients;
using AzureRepositories.EventLogs;
using AzureRepositories.PaymentSystems;
using AzureRepositories.VerificationCode;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Lykke.SettingsReader;

namespace AzureRepositories
{
    public static class AzureRepoFactories
    {
        private const string TableNameDictionaries = "Dictionaries";

        private const string TableNameEmailVerificationCodes = "EmailVerificationCodes";
        private const string TableNameSmsVerificationCodes = "SmsVerificationCodes";
        private const string TableNameSmsVerificationPriorityCodes = "SmsVerificationPriorityCodes";
        private const string TableNameEmailVerificationPriorityCodes = "EmailVerificationPriorityCodes";


        public static IdentityGenerator CreateIdentityGenerator(IReloadingManager<string> connstring, ILog log)
        {
            return new IdentityGenerator(AzureTableStorage<IdentityEntity>.Create(connstring, "Setup", log));
        }

        public static BrowserSessionsRepository CreateBrowserSessionsRepository(IReloadingManager<string> connString, ILog log)
        {
            return
                new BrowserSessionsRepository(AzureTableStorage<BrowserSessionEntity>.Create(connString, "BrowserSessions",
                    log));
        }

        public static PaymentTransactionsRepository CreateBankCardOrderRepository(IReloadingManager<string> connString, ILog log)
        {
            const string tableName = "PaymentTransactions";
            return new PaymentTransactionsRepository(AzureTableStorage<PaymentTransactionEntity>.Create(connString, tableName, log), AzureTableStorage<AzureMultiIndex>.Create(connString, tableName, log));
        }

        public static class Wallets
        {
            public static WalletsRepository CreateAccountsRepository(IReloadingManager<string> connString, ILog log)
            {
                return new WalletsRepository(AzureTableStorage<WalletEntity>.Create(connString, "Accounts", log));
            }
        }

        public static class EventLogs
        {
            public static PurchaseAttemptsLog CreatePurchaseLog(IReloadingManager<string> connectionString, ILog log)
            {
                return
                    new PurchaseAttemptsLog(AzureTableStorage<MarketOrderLogItemEntity>.Create(connectionString,
                        "PurchaseAttempts", log));
            }
        }

        public static class Bitcoin
        {
            public static BlockchainTransactionsCache CreateBlockchainTransactionsCache(IReloadingManager<string> connString, ILog log)
            {
                return
                    new BlockchainTransactionsCache(AzureTableStorage<ObsoleteBlockchainTransactionsCacheItem>.Create(connString,
                        "Transactions", log));
            }
        }

        public static class Clients
        {   
            public static AddressBookEntityRepository CreateAddressBookEntityRepository(IReloadingManager<string> connString, ILog log)
            {
                return
                    new AddressBookEntityRepository(AzureTableStorage<AddressBookEntity>.Create(connString, "AddressBookEntities", log));
            }

            public static AddressBookItemRepository CreateAddressBookItemRepository(IReloadingManager<string> connString, ILog log)
            {
                return
                    new AddressBookItemRepository(AzureTableStorage<AddressBookItem>.Create(connString, "AddressBookItems", log));
            }

            public static AddressBookPhoneNumbersWhiteListRepository CreateAddressBookPhoneNumbersWhiteListRepository(IReloadingManager<string> connString, ILog log)
            {
                return
                    new AddressBookPhoneNumbersWhiteListRepository(AzureTableStorage<AddressBookPhoneNumbersWhiteListEntity>.Create(connString, "AddressBookPhoneNumbersWhiteList", log));
            }

            public static PinSecurityRepository CreatePinSecurityRepository(IReloadingManager<string> connString, ILog log)
            {
                return new PinSecurityRepository(AzureTableStorage<PinSecurityEntity>.Create(connString, "ClientPins", log));
            }

            public static ClientBalanceChangeLogRepository CreateBalanceChangeLogRepository(IReloadingManager<string> connString, ILog log)
            {
                return
                    new ClientBalanceChangeLogRepository(
                        AzureTableStorage<ClientBalanceChangeLogRecordEntity>.Create(connString, "UpdateBalanceLog", log));
            }

            public static RecoveryTokensRepository CreateRecoveryTokensRepository(IReloadingManager<string> connString, ILog log)
            {
                return new RecoveryTokensRepository(
                    AzureTableStorage<RecoveryStatusEntity>.Create(connString, "RecoveryTokens", log),
                    AzureTableStorage<RecoveryTokenChallangeEntity>.Create(connString, "RecoveryTokens", log),
                    AzureTableStorage<RecoveryTokenLevel1Entity>.Create(connString, "RecoveryTokens", log));
            }
        }


        public static class BackOffice
        {
            public static MenuBadgesRepository CreateMenuBadgesRepository(IReloadingManager<string> connecionString, ILog log)
            {
                return
                    new MenuBadgesRepository(AzureTableStorage<MenuBadgeEntity>.Create(connecionString, "MenuBadges", log));
            }
        }
        
        public static class VerificationCodes
        {
            public static EmailVerificationCodeRepository CreateEmailVerificationRepository(IReloadingManager<string> connString, ILog log)
            {
                return new EmailVerificationCodeRepository(
                    AzureTableStorage<EmailVerificationCodeEntity>.Create(connString, TableNameEmailVerificationCodes, log),
                    AzureTableStorage<EmailVerificationPriorityCodeEntity>.Create(connString, TableNameEmailVerificationPriorityCodes, log));
            }

            public static SmsVerificationCodeRepository CreateSmsVerificationRepository(IReloadingManager<string> connString, ILog log)
            {
                return new SmsVerificationCodeRepository(
                    AzureTableStorage<SmsVerificationCodeEntity>.Create(connString, TableNameSmsVerificationCodes, log),
                    AzureTableStorage<SmsVerificationPriorityCodeEntity>.Create(connString, TableNameSmsVerificationPriorityCodes, log));
            }
        }

        public static class Applications
        {
            public static ApplicationRepository CreateApplicationsRepository(IReloadingManager<string> connstring, ILog log)
            {
                const string tableName = "Applications";
                return new ApplicationRepository(AzureTableStorage<ApplicationEntity>.Create(connstring, tableName, log));
            }
        }
    }
}
