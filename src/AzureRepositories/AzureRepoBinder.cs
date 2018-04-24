using Autofac;
using AutoMapper;
using AzureRepositories.BackOffice;
using AzureRepositories.Settings;
using AzureRepositories.Users;
using AzureStorage.Blob;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Cache;
using Common.Log;
using Core;
using Core.BackOffice;
using Core.Settings;
using Core.Users;
using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Logs;
using Lykke.SettingsReader;

namespace AzureRepositories
{
    public static class AzureRepoBinder
    {
        public static void MapEntities()
        {
            //Mapper.Configuration.AssertConfigurationIsValid();
        }

        public static void BindAzureRepositories(this ContainerBuilder container,
            IReloadingManager<DbSettings> dbSettings,
            ICacheManager cacheManager,
            ILog log)
        {
            MapEntities();

            container.RegisterInstance<IIdentityGenerator>(
                AzureRepoFactories.CreateIdentityGenerator(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IBrowserSessionsRepository>(
                AzureRepoFactories.CreateBrowserSessionsRepository(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), log));

            container.RegisterInstance<IAppGlobalSettingsRepositry>(
                new AppGlobalSettingsRepository(
                    AzureTableStorage<AppGlobalSettingsEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString), "Setup", log)));

            container.RegisterInstance<ILkkSourceWalletsRepository>(
                new LkkSourceWalletRepository(
                    AzureTableStorage<LkkSourceWalletEntity>.Create(dbSettings.ConnectionString(x => x.ClientPersonalInfoConnString),
                        "IcoLkkSourceWallets", log)));
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
