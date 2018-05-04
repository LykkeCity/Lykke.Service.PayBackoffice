using Autofac;
using AzureRepositories.BackOffice;
using AzureRepositories.Users;
using AzureStorage.Tables;
using Core;
using Core.BackOffice;
using Core.Users;

namespace AzureRepositories
{
    public static class AzureRepoMockBinder
    {

        public static void BindAzureReposInMem(this ContainerBuilder ioc)
        {
            var localHost = @"http://127.0.0.1:8998";
            
           ioc.RegisterInstance<IBrowserSessionsRepository>(
                new BrowserSessionsRepository(new AzureTableStorageLocal<BrowserSessionEntity>(localHost, "BrowserSessionsRepository")));

           ioc.RegisterInstance<IMenuBadgesRepository>(
                new MenuBadgesRepository(new AzureTableStorageLocal<MenuBadgeEntity>(localHost, "MenuBadgesRepository")));

           ioc.RegisterInstance<IBackOfficeUsersRepository>(
               new BackOfficeUsersRepository(new AzureTableStorageLocal<BackOfficeUserEntity>(localHost, "BackOfficeUsers")));

           ioc.RegisterInstance<IBackofficeUserRolesRepository>(
                new BackOfficeUserRolesRepository(new NoSqlTableInMemory<BackofficeUserRoleEntity>()));
        }

        public static void BindAzureReposInMemForTests(this ContainerBuilder ioc)
        {
           
           ioc.RegisterInstance<IBrowserSessionsRepository>(
                new BrowserSessionsRepository(new NoSqlTableInMemory<BrowserSessionEntity>()));

           ioc.RegisterInstance<IMenuBadgesRepository>(
                new MenuBadgesRepository(new NoSqlTableInMemory<MenuBadgeEntity>()));
        }
    }
}
