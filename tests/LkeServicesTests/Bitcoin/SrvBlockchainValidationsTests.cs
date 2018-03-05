using System.Threading.Tasks;
using Autofac;
using Core.Bitcoin;
using Core.BitCoin;
using LkeServices.Bitcoin;
using LkeServicesTests.Mocks;
using Moq;
using NUnit.Framework;

namespace LkeServicesTests.Bitcoin
{
    [TestFixture]
    public class SrvBlockchainValidationsTests
    {
        private IContainer _ioc;
        private SrvBlockchainValidations _srvBlockchainValidations;
        private IWalletCredentialsRepository _walletCredentialsRepository;

        [SetUp]
        public void SetUp()
        {
            var ioc = EnvironmentCreator.CreateEnvironment(stubForSrvBlockchainReader:false);

            var srvBlockchainReaderMock = new Mock<ISrvBlockchainReader>();

			srvBlockchainReaderMock.Setup(x => x.IsValidAddress(It.IsAny<string>(), It.IsAny<bool>()))
				.Returns(Task.FromResult(false));
			srvBlockchainReaderMock.Setup(x => x.IsValidAddress(It.IsIn("vdest", "cmsig", "msig"), It.IsAny<bool>()))
		        .Returns(Task.FromResult(true));	     
	        

	        srvBlockchainReaderMock.Setup(x => x.IsColoredAddress("cmsig")).Returns(Task.FromResult(true));
	        srvBlockchainReaderMock.Setup(x => x.IsColoredAddress("msig")).Returns(Task.FromResult(false));

			srvBlockchainReaderMock.Setup(x => x.GetUncoloredAdress(It.IsAny<string>()))
			   .Returns(Task.FromResult("any"));
			srvBlockchainReaderMock.Setup(x => x.GetUncoloredAdress("cmsig")).Returns(Task.FromResult("msig"));
	        srvBlockchainReaderMock.Setup(x => x.GetUncoloredAdress("msig")).Returns(Task.FromResult("msig"));
	       

			ioc.RegisterInstance(srvBlockchainReaderMock.Object);

	        _ioc = ioc.Build();
            _walletCredentialsRepository = _ioc.Resolve<IWalletCredentialsRepository>();
            _walletCredentialsRepository.SaveAsync(new WalletCredentials
            {
                ClientId = "clientId",
                ColoredMultiSig = "cmsig",
                MultiSig = "msig"
            });
            _srvBlockchainValidations = _ioc.Resolve<SrvBlockchainValidations>();
        }

        [Test]
        public async Task Is_cash_out_address_validated()
        {
            var validationErrors = await _srvBlockchainValidations.IsValidAddressToCashout("clientId", false, "vdest");
            var validationErrors2 = await _srvBlockchainValidations.IsValidAddressToCashout("clientId", false, "invalidDestination");

            Assert.AreEqual(ValidationErrors.None, validationErrors);
            Assert.AreEqual(ValidationErrors.InvalidAddress, validationErrors2);
        }

        [Test]
        public async Task Is_same_address_is_invalid_for_cash_out()
        {
            var validationErrors = await _srvBlockchainValidations.IsValidAddressToCashout("clientId", false, "cmsig");
            var validationErrors2 = await _srvBlockchainValidations.IsValidAddressToCashout("clientId", false, "msig");

            Assert.AreEqual(ValidationErrors.SameSourceAsDestination, validationErrors);
            Assert.AreEqual(ValidationErrors.SameSourceAsDestination, validationErrors2);
        }

        [Test]
        public async Task Is_сolored_expected_validation_for_cash_out_works()
        {
            var validationErrors = await _srvBlockchainValidations.IsValidAddressToCashout("clientId", true, "msig");

            Assert.AreEqual(ValidationErrors.ColoredAddressExpected, validationErrors);
        }
    }
}
