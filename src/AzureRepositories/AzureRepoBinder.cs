using Autofac;
using AutoMapper;
using AzureStorage.Tables;
using Common.Cache;
using Common.Log;
using Core.Settings;
using Lykke.Logs;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories
{
    public static class AzureRepoBinder
    {
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
    }
}
