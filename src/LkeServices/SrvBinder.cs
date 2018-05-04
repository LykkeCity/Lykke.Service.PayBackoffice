using Autofac;
using Core.Settings;
using LkeServices.Export;

namespace LkeServices
{
    public static class SrvBinder
    {

        public static void RegisterAllServices(this ContainerBuilder ioc)
        {           
            ioc.RegisterType<ExportService>().SingleInstance();
        }
    }
}
