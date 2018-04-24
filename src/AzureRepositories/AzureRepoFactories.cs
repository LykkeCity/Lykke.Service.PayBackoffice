using AzureRepositories.BackOffice;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;

namespace AzureRepositories
{
    public static class AzureRepoFactories
    {
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

        public static class BackOffice
        {
            public static MenuBadgesRepository CreateMenuBadgesRepository(IReloadingManager<string> connecionString, ILog log)
            {
                return
                    new MenuBadgesRepository(AzureTableStorage<MenuBadgeEntity>.Create(connecionString, "MenuBadges", log));
            }
        }
    }
}
