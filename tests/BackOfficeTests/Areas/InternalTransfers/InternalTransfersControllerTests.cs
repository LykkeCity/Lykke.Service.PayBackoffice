using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Areas.InternalTransfers.Controllers;
using BackOffice.Areas.InternalTransfers.Models.Request;
using BackOffice.Areas.InternalTransfers.Models.View;
using BackOfficeTests.Framework;
using Common.Log;
using Core.Accounts;
using Core.InternalTransfers;
using Core.Settings;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.ExchangeOperations.Client.AutorestClient.Models;
using Moq;
using NUnit.Framework;

namespace BackOfficeTests.Areas.InternalTransfers
{
    [TestFixture]
    public class InternalTransfersControllerTests : ControllersTestBase<InternalTransfersController>
    {
        private Mock<IExchangeOperationsServiceClient> _exchangeOperationsService;
        private Mock<ILog> _log;
        private Mock<IClientAccountClient> _clientAccountClient;
        private Mock<IInternalTransferRepository> _internalTransferRepository;
        private Mock<InternalTransfersSettings> _internalTransfersSettings;
        private Mock<IWalletsRepository> _walletsRepository;
        private Mock<IAssetsServiceWithCache> _assetsServiceWithCache;

        protected override InternalTransfersController InitController()
        {
            _exchangeOperationsService = new Mock<IExchangeOperationsServiceClient>();
            _exchangeOperationsService.Setup(x => x.TransferAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>()))
                .ReturnsAsync(new ExchangeOperationResult(0, ""));
            _log = new Mock<ILog>();

            _clientAccountClient = new Mock<IClientAccountClient>();

            _internalTransfersSettings = new Mock<InternalTransfersSettings>();

            _walletsRepository = new Mock<IWalletsRepository>();

            _internalTransferRepository = new Mock<IInternalTransferRepository>();

            _assetsServiceWithCache = new Mock<IAssetsServiceWithCache>();

            _assetsServiceWithCache = new Mock<IAssetsServiceWithCache>();
            var asset = Mock.Of<Asset>();
            asset.Accuracy = 100;
            _assetsServiceWithCache.Setup(x => x.TryGetAssetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(asset);

            return new InternalTransfersController(
                _exchangeOperationsService.Object,
                _log.Object,
                _clientAccountClient.Object,
                _internalTransferRepository.Object,
                _internalTransfersSettings.Object,
                _walletsRepository.Object,
                _assetsServiceWithCache.Object
            );
        }

        [Test]
        public async Task Transfer_WhenCalledForSameClients_ReturnsValidationError()
        {
            var internalTransferRequest = MakeRequest<InternalTransferRequest>(x =>
            {
                x.AccountTo = x.AccountFrom = GenerateId();
                x.Amount = 1;
            });

            var transferResult = await Controller.Transfer(internalTransferRequest);

            GetViewModel<TransferCompleteModel>(transferResult).Caption.ShouldBeEqualTo("Request is invalid: Receiver and Sender can't match.");
        }

        [Test]
        public async Task Transfer_WhenAmountIsZero_ReturnsValidationError()
        {
            var internalTransferRequest = MakeRequest<InternalTransferRequest>(x =>
            {
                x.AccountTo = GenerateId();
                x.AccountFrom = GenerateId();
                x.Amount = 0;
            });

            var transferResult = await Controller.Transfer(internalTransferRequest);

            GetViewModel<TransferCompleteModel>(transferResult).Caption.ShouldBeEqualTo("Request is invalid: Amount must a be positive number.");
        }


        [Test]
        public async Task Transfer_WhenCalledWithValidRequest_ExchangeServiceCalled()
        {
            var sender = GenerateId();
            var receiver = GenerateId();

            var internalTransferRequest = MakeRequest<InternalTransferRequest>(x =>
            {
                x.AccountFrom = sender;
                x.AccountTo = receiver;
                x.Amount = 24.5;
                x.AssetId = "USD";
                x.Comment = "Comment";
            });

            await Controller.Transfer(internalTransferRequest);

            _exchangeOperationsService.Verify(x => x.TransferAsync(receiver, sender, 24.5, "USD", "Common", null, null, null, null, 0.0));
        }

        [Test]
        public async Task Transfer_WhenLongNumber_NoExponentialForm()
        {
            var sender = GenerateId();
            var receiver = GenerateId();

            var internalTransferRequest = MakeRequest<InternalTransferRequest>(x =>
            {
                x.AccountFrom = sender;
                x.AccountTo = receiver;
                x.Amount = 0.000000000005;
                x.AssetId = "USD";
                x.Comment = "Comment";
            });

            var transferResult = await Controller.Transfer(internalTransferRequest);

            GetViewModel<TransferCompleteModel>(transferResult).Caption.ShouldBeEqualTo($"Transfer of 0.000000000005 USD from {sender} to {receiver} complete.");
        }

        private string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
