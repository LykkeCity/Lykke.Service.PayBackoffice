using Autofac;
using Autofac.Features.ResolveAnything;
using AzureRepositories;
using Common.Log;
using Core.Bitcoin;
using Core.BitCoin;
using Core.Settings;
using LkeServices;
using LkeServices.Http;
using LkeServices.ProcessModel;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Moq;

namespace LkeServicesTests.Mocks
{
	public static class EnvironmentCreator
	{
		public static ContainerBuilder CreateEnvironment(bool stubForBtcCommandProducer = true,
			bool stubCommandSender = true,
			bool stubForSrvBlockchainReader = true)
		{
			var ioc = new ContainerBuilder();

            ioc.RegisterType<ThreadSwitcherMock>().As<IThreadSwitcher>().SingleInstance();

            ioc.Register(x => new LogToMemory()).As<ILog>();

			ioc.Register(x => new Mock<IMatchingEngineClient>().Object).As<IMatchingEngineClient>();

			ioc.Register(x => new BaseSettings());

			ioc.BindAzureReposInMemForTests();
			if (stubForSrvBlockchainReader)
				ioc.Register(x=> new Mock<ISrvBlockchainReader>()).As<ISrvBlockchainReader>();

			ioc.BindCachedDicts();
			ioc.RegisterType<HttpRequestClient>();

			ioc.RegisterAllServices(new SupportToolsSettings());

			var srvBcnHelperStub = new Mock<ISrvBlockchainHelper>();		
			ioc.RegisterInstance(srvBcnHelperStub);

			ioc.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
			return ioc;
		}
	}




}
