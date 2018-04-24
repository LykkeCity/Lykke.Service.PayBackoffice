using Autofac;
using Core.Settings;
using LkeServices.Export;

namespace LkeServices
{
    public static class SrvBinder
    {
        public static void BindLykkeServicesApi(this ContainerBuilder ioc, LykkeServiceApiSettings serviceApiSettings)
        {
        }

        #region MatchingEngine

        public static void StartMatchingEngineServices(this ContainerBuilder ioc)
        {

        }

        #endregion

        public static void RegisterAllServices(this ContainerBuilder ioc, SupportToolsSettings supportToolsSettings)
        {
            // Settings
            ioc.RegisterInstance(supportToolsSettings);
            
            ioc.RegisterType<ExportService>().SingleInstance();
        }
    }
}
