using System;
using Microsoft.Extensions.DependencyInjection;
using Core.Partner;
using Lykke.Service.ClientAccount.Client;

namespace LkeDomainTest
{
    public static class Config
    {
        public static IServiceProvider ConfigureServices()
        {
            IServiceCollection serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            serviceCollection.AddSingleton<IClientAccountClient, Mock.MockClientAccountService>();
            serviceCollection.AddSingleton<IPartnerClientAccountRepository, Mock.MockPartnerClientAccountRepository>();
            serviceCollection.AddSingleton<IPartnerAccountPolicyRepository, Mock.MockPartnerAccountPolicyRepository>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}

