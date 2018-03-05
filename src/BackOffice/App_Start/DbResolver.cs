using Autofac;
using Common.IocContainer;
using Common.Log;
using Core.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using Core.Settings;
using Lykke.MatchingEngine.Connector.Services;

namespace BackOffice
{
	public static class Dependencies
	{
		public static IBackOfficeUsersRepository BackOfficeUsersRepository { get; private set; }
		public static IBackofficeUserRolesRepository BackofficeUserRolesRepository { get; private set; }
	    public static TcpMatchingEngineClient TcpMeClient { get; private set; }

        private static void CreateAdminUser(IContainer ioc)
		{
			var usersRepo = ioc.Resolve<IBackOfficeUsersRepository>();
		    var googleAuthSettings = ioc.Resolve<GoogleAuthSettings>();

			var adminUser = usersRepo.UserExists(googleAuthSettings.DefaultAdminEmail).Result;
		    if (!adminUser)
		        usersRepo.CreateAsync(googleAuthSettings.DefaultAdminEmail, "Admin", true, new string[0]).Wait();
		}

        private static void InitSingletones(IContainer ioc)
		{
			BackOfficeUsersRepository = ioc.Resolve<IBackOfficeUsersRepository>();
			BackofficeUserRolesRepository = ioc.Resolve<IBackofficeUserRolesRepository>();
			TcpMeClient = ioc.Resolve<TcpMatchingEngineClient>();
		}

		public static IContainer BindDependecies(IServiceCollection collection, IConfigurationRoot configuration)
		{
			var provider = collection.BuildServiceProvider();

			var ioc = new ContainerBuilder();

            ioc.Populate(collection);

		    provider.GetService<IDependencyBinder>().Bind(configuration, ioc);

            var container = ioc.Build();

            CreateAdminUser(container);
			InitSingletones(container);

            container.Resolve<ILog>().WriteInfoAsync("DResolver", "Binding", "", "App Stated");

		    return container;
		}
	}
}
