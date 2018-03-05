using Core.Partner;
using LkeDomain.Credentials;
using NUnit.Framework;
using Lykke.Service.ClientAccount.Client;

namespace LkeDomainTest.Credentials
{
    [TestFixture]
    public class ClientAccountLogicTest
    {
        private IClientAccountClient _clientAccountService;
        private IPartnerAccountPolicyRepository _artnerAccountPolicyRepo;

        [OneTimeSetUp]
        public void TestInit()
        {
            var serviceProvider = Config.ConfigureServices();

            _clientAccountService = serviceProvider.GetService(typeof(IClientAccountClient)) as IClientAccountClient;
            _artnerAccountPolicyRepo = serviceProvider.GetService(typeof(IPartnerAccountPolicyRepository)) as IPartnerAccountPolicyRepository;
        }

        [Test]
        public void ClientAccountLogic_DoNotUsePartnerPolicies_Test()
        {
            ClientAccountLogic clientAccountLogic = new ClientAccountLogic(_clientAccountService, _artnerAccountPolicyRepo);

            string publicId = "publicId_{1}";

            bool result = clientAccountLogic.UsePartnerCredentials(publicId).Result;

            Assert.IsFalse(result);
        }

        [Test]
        public void ClientAccountLogic_UsePartnerPolicies_Test()
        {
            ClientAccountLogic clientAccountLogic = new ClientAccountLogic(_clientAccountService, _artnerAccountPolicyRepo);

            string publicId = "publicId1";

            bool result = clientAccountLogic.UsePartnerCredentials(publicId).Result;

            Assert.IsTrue(result);
        }
    }
}
