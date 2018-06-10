using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.IocContainer;
using Common.Log;
using Lykke.Service.BackofficeMembership.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackOffice
{
    public static class Dependencies
	{
		public static IBackofficeMembershipClient BackofficeMembershipClient { get; private set; }

        private static void InitSingletones(IContainer ioc)
		{
			BackofficeMembershipClient = ioc.Resolve<IBackofficeMembershipClient>();
			//TcpMeClient = ioc.Resolve<TcpMatchingEngineClient>();
		}

		public static IContainer BindDependecies(IServiceCollection collection, IConfigurationRoot configuration)
		{
			var provider = collection.BuildServiceProvider();

			var ioc = new ContainerBuilder();

            ioc.Populate(collection);

		    provider.GetService<IDependencyBinder>().Bind(configuration, ioc);

            var container = ioc.Build();

			InitSingletones(container);

            container.Resolve<ILog>().WriteInfoAsync("DResolver", "Binding", "", "App Stated");

		    return container;
		}
	}
}
