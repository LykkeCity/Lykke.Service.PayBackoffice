using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.Bitcoin.Api.Client.BitcoinApi.Models;
using Common;
using Common.Log;
using Core;
using Core.Accounts;
using Core.Assets;
using Core.BitCoin;
using Core.Exceptions;
using Core.Exchange;
using Core.Offchain;
using Core.Offchain.Models;
using Core.Settings;
using LkeDomain.Extensions;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using ME = Lykke.MatchingEngine.Connector;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.RateCalculator.Client;
using Lykke.Service.RateCalculator.Client.AutorestClient.Models;
using OrderAction = Core.Exchange.OrderAction;
using AssetPair = Lykke.Service.Assets.Client.Models.AssetPair;
using ErrorResponse = Lykke.Bitcoin.Api.Client.BitcoinApi.Models.ErrorResponse;

namespace LkeServices.Offchain
{
    public class OffchainService : IOffchainService
    {
        private const double AdditionalPercentForOrder = 2;

        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IBitcoinApiClient _bitcoinApiClient;
        private readonly ILog _logger;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IOffchainTransferRepository _offchainTransferRepository;
        private readonly IOffchainOrdersRepository _offchainOrdersRepository;
        private readonly OffchainSettings _offchainSettings;
        private readonly Core.Settings.ExchangeSettings _exchangeSettings;
        private readonly CachedDataDictionary<string, AssetPair> _assetPairs;
        private readonly CachedTradableAssetsDictionary _tradableAssets;
        private readonly IRateCalculatorClient _rateCalculatorClient;
        private readonly IOffchainEncryptedKeysRepository _offchainEncryptedKeysRepository;
        private readonly IOffchainRequestService _offchainRequestService;
        private readonly IWalletsRepository _walletsRepository;
        private readonly IOffchainFinalizeCommandProducer _offchainFinalizeCommandProducer;
        private readonly IOrderBooksService _orderBooksService;
        private readonly IBitcoinTransactionService _bitcoinTransactionService;
        private readonly IClientAccountClient _clientAccountClient;
        private readonly ILimitOrdersRepository _limitOrdersRepository;

        public OffchainService(
            IBitcoinApiClient bitcoinApiClient,
            ILog logger,
            IWalletCredentialsRepository walletCredentialsRepository,
            OffchainSettings offchainSettings,
            Core.Settings.ExchangeSettings exchangeSettings,
            IOffchainTransferRepository offchainTransferRepository,
            IOffchainOrdersRepository offchainOrdersRepository,
            CachedDataDictionary<string, AssetPair> assetPairs,
            IRateCalculatorClient rateCalculatorClient,
            IMatchingEngineClient matchingEngineClient,
            IOffchainEncryptedKeysRepository offchainEncryptedKeysRepository,
            IOffchainRequestService offchainRequestService,
            CachedTradableAssetsDictionary tradableAssets,
            IWalletsRepository walletsRepository,
            IOffchainFinalizeCommandProducer offchainFinalizeCommandProducer,
            IOrderBooksService orderBooksService,
            IBitcoinTransactionService bitcoinTransactionService, IClientAccountClient clientAccountClient, ILimitOrdersRepository limitOrdersRepository)
        {
            _bitcoinApiClient = bitcoinApiClient;
            _logger = logger;
            _walletCredentialsRepository = walletCredentialsRepository;
            _offchainSettings = offchainSettings;
            _exchangeSettings = exchangeSettings;
            _offchainTransferRepository = offchainTransferRepository;
            _offchainOrdersRepository = offchainOrdersRepository;
            _assetPairs = assetPairs;
            _rateCalculatorClient = rateCalculatorClient;
            _matchingEngineClient = matchingEngineClient;
            _offchainEncryptedKeysRepository = offchainEncryptedKeysRepository;
            _offchainRequestService = offchainRequestService;
            _tradableAssets = tradableAssets;
            _walletsRepository = walletsRepository;
            _offchainFinalizeCommandProducer = offchainFinalizeCommandProducer;
            _orderBooksService = orderBooksService;
            _bitcoinTransactionService = bitcoinTransactionService;
            _clientAccountClient = clientAccountClient;
            _limitOrdersRepository = limitOrdersRepository;
        }

        public async Task<OffchainResult> CreateTransfer(string clientId, string assetPair, string asset, decimal amount, string prevTempPrivateKey)
        {
            decimal neededAmount;
            string neededAsset;

            var assetPairModel = await _assetPairs.GetItemAsync(assetPair);
            var orderAction = amount > 0 ? OrderAction.Buy : OrderAction.Sell;

            Asset assetModel;

            if (orderAction == OrderAction.Buy)
            {
                neededAsset = assetPairModel.BaseAssetId == asset ? assetPairModel.QuotingAssetId : assetPairModel.BaseAssetId;

                var receivedAsset = assetPairModel.BaseAssetId == asset ? assetPairModel.BaseAssetId : assetPairModel.QuotingAssetId;

                neededAmount = await GetExchangeAmount(receivedAsset, neededAsset, amount, orderAction);

                if (neededAmount == 0 || neededAsset == LykkeConstants.BitcoinAssetId && neededAmount < (decimal)_exchangeSettings.MinBtcOrderAmount)
                    throw new OffchainException(ErrorCode.LowVolume, neededAsset);

                var wallet = await _walletsRepository.GetAsync(clientId, neededAsset);

                if (wallet == null)
                    throw new OffchainException(ErrorCode.NotEnoughtClientFunds, neededAsset);

                assetModel = await _tradableAssets.GetItemAsync(neededAsset);

                // + 2%
                neededAmount = neededAmount * (decimal)(1 + AdditionalPercentForOrder / 100.0);

                neededAmount = Math.Min((decimal)wallet.Balance, neededAmount.TruncateDecimalPlaces(assetModel.Accuracy, true));
            }
            else
            {
                neededAmount = Math.Abs(amount);
                neededAsset = assetPairModel.BaseAssetId == asset ? assetPairModel.BaseAssetId : assetPairModel.QuotingAssetId;

                var wallet = await _walletsRepository.GetAsync(clientId, neededAsset);
                assetModel = await _tradableAssets.GetItemAsync(neededAsset);

                neededAmount = Math.Min((decimal)wallet.Balance, neededAmount.TruncateDecimalPlaces(assetModel.Accuracy, true));
            }

            var credentials = await _walletCredentialsRepository.GetAsync(clientId);

            var result = assetModel.IsTrusted
                            ? GetFakeResultForTrustedAsset()
                            : await _bitcoinApiClient.OffchainTransferAsync(new OffchainTransferData
                            {
                                Amount = -neededAmount,
                                AssetId = neededAsset,
                                ClientPrevPrivateKey = prevTempPrivateKey,
                                ClientPubKey = credentials.PublicKey,
                                Required = false
                            });

            var offchainOrder = await _offchainOrdersRepository.CreateOrder(clientId, asset, assetPair, amount, neededAmount, assetPairModel.BaseAssetId == asset);

            var offchainTransfer = await _offchainTransferRepository.CreateTransfer(Guid.NewGuid().ToString(), clientId, neededAsset, neededAmount, OffchainTransferType.FromClient, result.TransferId?.ToString(), offchainOrder.Id);

            if (result.HasError)
                return await InternalErrorProcessing(nameof(CreateTransfer), result.Error, credentials, offchainTransfer, false);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = OffchainOperationResult.Transfer
            };
        }

        public async Task<OffchainResult> CreateLimitTransfer(string clientId, string assetPair, string asset, decimal amount, decimal price, string prevTempPrivateKey)
        {
            decimal neededAmount;
            string neededAsset;

            var assetPairModel = await _assetPairs.GetItemAsync(assetPair);
            var orderAction = amount > 0 ? OrderAction.Buy : OrderAction.Sell;

            await CheckOrderPrice(assetPair, orderAction, price);

            Asset assetModel;

            if (orderAction == OrderAction.Buy)
            {
                neededAsset = assetPairModel.BaseAssetId == asset ? assetPairModel.QuotingAssetId : assetPairModel.BaseAssetId;

                neededAmount = assetPairModel.BaseAssetId == asset ? amount * price : amount / price;

                if (neededAmount == 0 || neededAsset == LykkeConstants.BitcoinAssetId && neededAmount < (decimal)_exchangeSettings.MinBtcOrderAmount)
                    throw new OffchainException(ErrorCode.LowVolume, neededAsset);

                var wallet = await _walletsRepository.GetAsync(clientId, neededAsset);

                assetModel = await _tradableAssets.GetItemAsync(neededAsset);

                neededAmount = neededAmount.TruncateDecimalPlaces(assetModel.Accuracy, true);

                if (wallet == null || (decimal)wallet.Balance < neededAmount)
                    throw new OffchainException(ErrorCode.NotEnoughtClientFunds, neededAsset);
            }
            else
            {
                neededAmount = Math.Abs(amount);
                neededAsset = assetPairModel.BaseAssetId == asset ? assetPairModel.BaseAssetId : assetPairModel.QuotingAssetId;

                var wallet = await _walletsRepository.GetAsync(clientId, neededAsset);
                assetModel = await _tradableAssets.GetItemAsync(neededAsset);

                neededAmount = Math.Min((decimal)wallet.Balance, neededAmount.TruncateDecimalPlaces(assetModel.Accuracy, true));
            }

            var credentials = await _walletCredentialsRepository.GetAsync(clientId);

            var result = assetModel.IsTrusted
                                    ? GetFakeResultForTrustedAsset()
                                    : await _bitcoinApiClient.OffchainTransferAsync(new OffchainTransferData
                                    {
                                        Amount = -neededAmount,
                                        AssetId = neededAsset,
                                        ClientPrevPrivateKey = prevTempPrivateKey,
                                        ClientPubKey = credentials.PublicKey,
                                        Required = false
                                    });

            var offchainOrder = await _offchainOrdersRepository.CreateLimitOrder(clientId, asset, assetPair, amount, neededAmount, assetPairModel.BaseAssetId == asset, price);

            var offchainTransfer = await _offchainTransferRepository.CreateTransfer(Guid.NewGuid().ToString(), clientId, neededAsset, neededAmount, OffchainTransferType.FromClient, result.TransferId?.ToString(), offchainOrder.Id);

            if (result.HasError)
                return await InternalErrorProcessing(nameof(CreateLimitTransfer), result.Error, credentials, offchainTransfer, false);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = OffchainOperationResult.Transfer
            };
        }

        public async Task<OffchainResult> CreateOffchainCashout(string clientId, string asset, decimal amount, string prevTempPrivateKey)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);
            var assetModel = await _tradableAssets.GetItemAsync(asset);

            var result = assetModel.IsTrusted
                                    ? GetFakeResultForTrustedAsset()
                                    : await _bitcoinApiClient.OffchainTransferAsync(new OffchainTransferData
                                    {
                                        Amount = -amount,
                                        AssetId = asset,
                                        ClientPrevPrivateKey = prevTempPrivateKey,
                                        ClientPubKey = credentials.PublicKey,
                                        Required = false
                                    });

            var offchainTransfer = await _offchainTransferRepository.CreateTransfer(Guid.NewGuid().ToString(), clientId, asset, amount, OffchainTransferType.OffchainCashout, result.TransferId?.ToString(), null);

            if (result.HasError)
                return await InternalErrorProcessing(nameof(CreateTransfer), result.Error, credentials, offchainTransfer, false);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = OffchainOperationResult.Transfer
            };
        }

        public async Task<OffchainResult> CreateDirectTransfer(Guid id, string clientId, string asset, decimal amount, string prevTempPrivateKey)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);
            var assetModel = await _tradableAssets.GetItemAsync(asset);

            var result = assetModel.IsTrusted
                                    ? GetFakeResultForTrustedAsset()
                                    : await _bitcoinApiClient.OffchainTransferAsync(new OffchainTransferData
                                    {
                                        Amount = -amount,
                                        AssetId = asset,
                                        ClientPrevPrivateKey = prevTempPrivateKey,
                                        ClientPubKey = credentials.PublicKey,
                                        Required = false
                                    });

            var offchainTransfer = await _offchainTransferRepository.CreateTransfer(id.ToString(), clientId, asset, amount, OffchainTransferType.DirectTransferFromClient, result.TransferId?.ToString(), null);

            if (result.HasError)
                return await InternalErrorProcessing(nameof(CreateTransfer), result.Error, credentials, offchainTransfer, false);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = OffchainOperationResult.Transfer
            };
        }

        public async Task<OffchainResult> TransferRequest(string clientId, string transferId, string prevTempPrivateKey)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);

            var offchainTransfer = await _offchainTransferRepository.GetTransfer(transferId);

            if (offchainTransfer == null || offchainTransfer.ClientId != clientId || offchainTransfer.Completed)
                throw new OffchainException(ErrorCode.Exception, offchainTransfer?.AssetId);

            var assetModel = await _tradableAssets.GetItemAsync(offchainTransfer.AssetId);

            if (offchainTransfer.Type == OffchainTransferType.CashinFromClient && !assetModel.IsTrusted)
                return await CreateChannel(credentials, offchainTransfer, true);
            if (offchainTransfer.Type == OffchainTransferType.HubCashout && !assetModel.IsTrusted)
                return await HubCashout(credentials, offchainTransfer);
            if (offchainTransfer.Type == OffchainTransferType.TrustedCashout)
                return await TrustedCashout(credentials, offchainTransfer);

            var result = assetModel.IsTrusted
                                    ? GetFakeResultForTrustedAsset()
                                    : await _bitcoinApiClient.OffchainTransferAsync(new OffchainTransferData
                                    {
                                        Amount = offchainTransfer.Amount,
                                        AssetId = offchainTransfer.AssetId,
                                        ClientPrevPrivateKey = prevTempPrivateKey,
                                        ClientPubKey = credentials.PublicKey,
                                        Required = true,
                                        ExternalTransferId = offchainTransfer.ExternalTransferId
                                    });

            if (result.HasError)
                return await InternalErrorProcessing(nameof(TransferRequest), result.Error, credentials, offchainTransfer, true);

            await _offchainTransferRepository.UpdateTransfer(transferId, result.TransferId?.ToString());

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = OffchainOperationResult.Transfer
            };
        }

        public async Task<OffchainResult> CreateChannel(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, bool required)
        {
            if (offchainTransfer == null || offchainTransfer.ClientId != credentials.ClientId || offchainTransfer.Completed)
                throw new OffchainException(ErrorCode.Exception, offchainTransfer?.AssetId);

            var fromClient = offchainTransfer.Type == OffchainTransferType.FromClient ||
                             offchainTransfer.Type == OffchainTransferType.DirectTransferFromClient ||
                             offchainTransfer.Type == OffchainTransferType.OffchainCashout;

            var fromHub = offchainTransfer.Type == OffchainTransferType.FromHub ||
                          offchainTransfer.Type == OffchainTransferType.CashinToClient;

            var result = await _bitcoinApiClient.CreateChannelAsync(new CreateChannelData
            {
                AssetId = offchainTransfer.AssetId,
                ClientPubKey = credentials.PublicKey,
                ClientAmount = fromClient ? offchainTransfer.Amount : 0,
                HubAmount = fromHub ? offchainTransfer.Amount : 0,
                Required = required,
                ExternalTransferId = offchainTransfer.ExternalTransferId
            });

            if (!result.HasError)
            {
                await _offchainTransferRepository.UpdateTransfer(offchainTransfer.Id, result.TransferId?.ToString(), closing: result.ChannelClosing, onchain: true);

                return new OffchainResult
                {
                    TransferId = offchainTransfer.Id,
                    TransactionHex = result.Transaction,
                    OperationResult = result.ChannelClosing ? OffchainOperationResult.Transfer : OffchainOperationResult.CreateChannel
                };
            }

            await _logger.WriteWarningAsync("OffchainService", "CreateChannel", $"Client: [{credentials.ClientId}], error: [{result.Error.ErrorCode}], transfer: [{offchainTransfer.Id}], message: [{result.Error.Message}]");

            throw new OffchainException(result.Error.ErrorCode, offchainTransfer.AssetId);
        }

        private async Task<OffchainResult> TrustedCashout(IWalletCredentials credentials, IOffchainTransfer offchainTransfer)
        {
            var result = await _bitcoinApiClient.FullCashout(new HubCashoutData()
            {
                AssetId = offchainTransfer.AssetId,
                ClientPubKey = credentials.PublicKey
            });

            if (result.HasError)
            {
                if (result.Error.ErrorCode == ErrorCode.ShouldOpenNewChannel ||
                    result.Error.ErrorCode == ErrorCode.NoCoinsToRefund)
                {
                    throw new OffchainException(ErrorCode.NoCoinsToRefund, offchainTransfer.AssetId);
                }

                await _logger.WriteErrorAsync("OffchainService", "TrustedCashout", $"Client: [{credentials.ClientId}], error: [{result.Error.ErrorCode}]", new Exception(result.Error.Message));
                throw new OffchainException(result.Error.ErrorCode, offchainTransfer.AssetId);
            }

            await _offchainTransferRepository.UpdateTransfer(offchainTransfer.Id, result.TransferId?.ToString(), result.ChannelClosing, true);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = result.ChannelClosing ? OffchainOperationResult.Transfer : OffchainOperationResult.CreateChannel
            };
        }

        public async Task<OffchainResult> HubCashout(IWalletCredentials credentials, IOffchainTransfer offchainTransfer)
        {
            var result = await _bitcoinApiClient.HubCashout(new HubCashoutData()
            {
                AssetId = offchainTransfer.AssetId,
                ClientPubKey = credentials.PublicKey
            });

            if (result.HasError)
            {
                if (result.Error.ErrorCode == ErrorCode.ShouldOpenNewChannel ||
                    result.Error.ErrorCode == ErrorCode.NoCoinsToRefund)
                {
                    throw new OffchainException(ErrorCode.NoCoinsToRefund, offchainTransfer.AssetId);
                }

                await _logger.WriteWarningAsync("OffchainService", "CreateCashout", $"Client: [{credentials.ClientId}], error: [{result.Error.ErrorCode}]");
                throw new OffchainException(result.Error.ErrorCode, offchainTransfer.AssetId);
            }

            await _offchainTransferRepository.UpdateTransfer(offchainTransfer.Id, result.TransferId?.ToString(), result.ChannelClosing, true);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = result.ChannelClosing ? OffchainOperationResult.Transfer : OffchainOperationResult.CreateChannel
            };
        }

        public async Task<OffchainResult> CreateHubCommitment(string clientId, string transferId, string signedChannel)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);
            var offchainTransfer = await _offchainTransferRepository.GetTransfer(transferId);

            if (offchainTransfer.Completed || offchainTransfer.ClientId != clientId)
                throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);

            var amount = 0.0M;
            var required = false;
            switch (offchainTransfer.Type)
            {
                case OffchainTransferType.DirectTransferFromClient:
                case OffchainTransferType.OffchainCashout:
                case OffchainTransferType.FromClient:
                    amount = -offchainTransfer.Amount;
                    break;
                case OffchainTransferType.CashinToClient:
                case OffchainTransferType.FromHub:
                    amount = offchainTransfer.Amount;
                    required = true;
                    break;
            }

            var result = await _bitcoinApiClient.CreateHubCommitment(new CreateHubComitmentData
            {
                Amount = amount,
                ClientPubKey = credentials.PublicKey,
                AssetId = offchainTransfer.AssetId,
                SignedByClientChannel = signedChannel
            });

            if (result.HasError)
                return await InternalErrorProcessing(nameof(ProcessClientTransfer), result.Error, credentials, offchainTransfer, required);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = OffchainOperationResult.Transfer
            };
        }

        public async Task<OffchainResult> Finalize(string clientId, string transferId, string clientRevokePubKey, string clientRevokeEncryptedPrivateKey, string signedCommitment)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);
            var offchainTransfer = await _offchainTransferRepository.GetTransfer(transferId);

            if (offchainTransfer.Completed || offchainTransfer.ClientId != clientId)
                throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);

            switch (offchainTransfer.Type)
            {
                case OffchainTransferType.FromClient:
                    return await ProcessClientTransfer(credentials, offchainTransfer, clientRevokePubKey,
                        clientRevokeEncryptedPrivateKey, signedCommitment);
                case OffchainTransferType.FromHub:
                case OffchainTransferType.CashinFromClient:
                case OffchainTransferType.CashinToClient:
                case OffchainTransferType.DirectTransferFromClient:
                    return await ProcessTransfer(credentials, offchainTransfer, clientRevokePubKey,
                        clientRevokeEncryptedPrivateKey, signedCommitment);
                case OffchainTransferType.OffchainCashout:
                case OffchainTransferType.ClientCashout:
                case OffchainTransferType.HubCashout:
                case OffchainTransferType.TrustedCashout:
                    {
                        if (offchainTransfer.ChannelClosing)
                            return await ProcessChannelClosing(credentials, offchainTransfer, signedCommitment);
                        return await ProcessCashout(credentials, offchainTransfer, clientRevokePubKey, clientRevokeEncryptedPrivateKey, signedCommitment);
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<OffchainResultOrder> GetResultOrderFromTransfer(string transferId)
        {
            var offchainTransfer = await _offchainTransferRepository.GetTransfer(transferId);

            if (string.IsNullOrWhiteSpace(offchainTransfer.OrderId))
                return null;

            var offchainOrder = await _offchainOrdersRepository.GetOrder(offchainTransfer.OrderId);

            if (offchainOrder.Price == 0)
                return null;

            var assetPair = await _assetPairs.GetItemAsync(offchainOrder.AssetPair);
            var otherAsset = await _tradableAssets.GetItemAsync(assetPair.BaseAssetId == offchainOrder.Asset ? assetPair.QuotingAssetId : assetPair.BaseAssetId);

            var orderBuy = offchainOrder.Volume > 0;

            var price = offchainOrder.Straight ? offchainOrder.Price : 1 / offchainOrder.Price;
            var rate = ((double)price).TruncateDecimalPlaces(offchainOrder.Straight ? assetPair.Accuracy : assetPair.InvertedAccuracy, orderBuy);

            var converted = (rate * (double)offchainOrder.Volume).TruncateDecimalPlaces(otherAsset.Accuracy, orderBuy);

            var totalCost = orderBuy ? converted : (double)offchainOrder.Volume;
            var volume = orderBuy ? (double)offchainOrder.Volume : converted;

            return new OffchainResultOrder
            {
                Asset = offchainOrder.Asset,
                Id = offchainOrder.Id,
                AssetPair = offchainOrder.AssetPair,
                Volume = volume,
                Price = rate,
                TotalCost = totalCost,
                DateTime = offchainOrder.CreatedAt,
                OrderType = offchainOrder.Volume > 0 ? OrderType.Buy : OrderType.Sell
            };
        }

        public async Task<bool> CancelLimitOrder(string clientId, string orderId)
        {
            var response = await _matchingEngineClient.CancelLimitOrderAsync(orderId);

            if (response.Status == MeStatusCodes.Ok)
            {
                return true;
            }

            if (response.Status == MeStatusCodes.NotFound)
            {
                await _logger.WriteWarningAsync(nameof(OffchainService), nameof(CancelLimitOrder), $"Client: {clientId}, order: {orderId}, status: {response.Status}");
                return true;
            }

            await _logger.WriteErrorAsync(nameof(OffchainService), nameof(CancelLimitOrder), $"Client: {clientId}, order: {orderId}, status: {response.Status}", new Exception(response.Message));

            return false;
        }

        public async Task<OffchainResult> CreateCashout(string clientId, string destination, string asset, decimal amount)
        {
            var credentials = await _walletCredentialsRepository.GetAsync(clientId);
            var assetModel = await _tradableAssets.GetItemAsync(asset);

            var result = assetModel.IsTrusted
                ? GetFakeClosingResultForTrustedAsset()
                : await _bitcoinApiClient.Cashout(new CashoutData
                {
                    AssetId = asset,
                    ClientPubKey = credentials.PublicKey,
                    Amount = amount,
                    HotWalletAddress = _offchainSettings.HotWalletAddress,
                    CashoutAddress = destination
                });

            if (result.HasError)
            {
                await _logger.WriteWarningAsync("OffchainService", "CreateCashout", $"Client: [{clientId}], error: [{result.Error.ErrorCode}]");
                throw new OffchainException(result.Error.ErrorCode, asset);
            }

            var offchainTransfer = await _offchainTransferRepository.CreateTransfer(Guid.NewGuid().ToString(), clientId, asset, amount, OffchainTransferType.ClientCashout, result.TransferId?.ToString(), null, result.ChannelClosing);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = result.Transaction,
                OperationResult = result.ChannelClosing ? OffchainOperationResult.Transfer : OffchainOperationResult.CreateChannel
            };
        }

        private async Task<OffchainResult> ProcessClientTransfer(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, string revokePubKey, string encryptedRevokePrivakeKey, string signedCommitment)
        {
            OffchainBaseResponse result;
            var assetModel = await _tradableAssets.GetItemAsync(offchainTransfer.AssetId);

            if (assetModel.IsTrusted)
            {
                result = GetFakeResultForTrustedAsset();
            }
            else if (offchainTransfer.ChannelClosing)
            {
                result = await _bitcoinApiClient.CloseChannel(new CloseChannelData
                {
                    ClientPubKey = credentials.PublicKey,
                    AssetId = offchainTransfer.AssetId,
                    SignedClosingTransaction = signedCommitment,
                    OffchainTransferId = offchainTransfer.Id
                });
            }
            else
            {
                result = await _bitcoinApiClient.Finalize(new FinalizeData
                {
                    ClientPubKey = credentials.PublicKey,
                    AssetId = offchainTransfer.AssetId,
                    ClientRevokePubKey = revokePubKey,
                    SignedByClientHubCommitment = signedCommitment,
                    ExternalTransferId = offchainTransfer.ExternalTransferId,
                    OffchainTransferId = offchainTransfer.Id
                });
            }

            await _offchainEncryptedKeysRepository.UpdateKey(credentials.ClientId, offchainTransfer.AssetId, encryptedRevokePrivakeKey);

            if (result.HasError)
                return await InternalErrorProcessing(nameof(ProcessClientTransfer), result.Error, credentials, offchainTransfer, false);

            await _offchainTransferRepository.CompleteTransfer(offchainTransfer.Id, blockchainHash: result.TxHash);

            var order = await _offchainOrdersRepository.GetOrder(offchainTransfer.OrderId);

            var meOrderAction = order.Volume > 0
                ? ME.Abstractions.Models.OrderAction.Buy
                : ME.Abstractions.Models.OrderAction.Sell;

            MeStatusCodes? status = null;
            try
            {
                // save context for tx handler
                var ctx = new SwapOffchainContextData();
                ctx.Operations.Add(new SwapOffchainContextData.Operation
                {
                    Amount = -offchainTransfer.Amount,
                    AssetId = offchainTransfer.AssetId,
                    ClientId = offchainTransfer.ClientId,
                    TransactionId = offchainTransfer.Id
                });
                await _bitcoinTransactionService.SetTransactionContext(offchainTransfer.OrderId, ctx);

                if (order.IsLimit)
                {
                    var response = await _matchingEngineClient.PlaceLimitOrderAsync(
                        new LimitOrderModel
                        {
                            Id = order.Id,
                            ClientId = credentials.ClientId,
                            AssetPairId = order.AssetPair,
                            OrderAction = meOrderAction,
                            Volume = (double)Math.Abs(order.Volume),
                            Price = (double)order.Price,
                        });

                    status = response?.Status;

                    if (status == MeStatusCodes.Ok)
                    {
                        return new OffchainResult
                        {
                            TransferId = offchainTransfer.Id,
                            TransactionHex = "0x0", //result.Transaction,
                            OperationResult = OffchainOperationResult.ClientCommitment
                        };
                    }
                }
                else
                {
                    var response = await _matchingEngineClient.HandleMarketOrderAsync(
                        new MarketOrderModel
                        {
                            Id = order.Id,
                            ClientId = credentials.ClientId,
                            AssetPairId = order.AssetPair,
                            OrderAction = meOrderAction,
                            Volume = (double)Math.Abs(order.Volume),
                            Straight = order.Straight,
                            ReservedLimitVolume = (double)offchainTransfer.Amount,
                        });

                    status = response?.Status;

                    if (status == MeStatusCodes.Ok)
                    {
                        await _offchainOrdersRepository.UpdatePrice(order.Id, (decimal)response.Price);

                        return new OffchainResult
                        {
                            TransferId = offchainTransfer.Id,
                            TransactionHex = "0x0", //result.Transaction,
                            OperationResult = OffchainOperationResult.ClientCommitment
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.WriteErrorAsync("OffchainService", "Finalize", $"Client: [{credentials.ClientId}], error: ME failed, order: [{order.Id}], transfer: [{offchainTransfer.Id}]", ex);
            }

            // reverse client transaction if ME returns error
            if (!assetModel.IsTrusted)
                await _offchainRequestService.CreateOffchainRequest(Guid.NewGuid().ToString(), credentials.ClientId, offchainTransfer.AssetId, offchainTransfer.Amount, order.Id, OffchainTransferType.FromHub);

            await _logger.WriteWarningAsync("OffchainService", "Finalize", $"Client: [{credentials.ClientId}], error: ME failed, order: [{order.Id}], transfer: [{offchainTransfer.Id}]");

            if (status != null)
            {
                if (status == MeStatusCodes.LeadToNegativeSpread)
                    throw new TradeException(TradeExceptionType.LeadToNegativeSpread);
                if (status == MeStatusCodes.NotEnoughFunds)
                    throw new OffchainException(ErrorCode.NotEnoughtClientFunds, offchainTransfer.AssetId);
                if (status == MeStatusCodes.Dust)
                    throw new OffchainException(ErrorCode.LowVolume, offchainTransfer.AssetId);
                if (status == MeStatusCodes.NoLiquidity)
                    throw new OffchainException(offchainTransfer.AssetId == LykkeConstants.BitcoinAssetId ? ErrorCode.NotEnoughBitcoinAvailable : ErrorCode.NotEnoughAssetAvailable, offchainTransfer.AssetId);
            }

            throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);
        }

        private async Task<OffchainResult> ProcessTransfer(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, string revokePubKey, string encryptedRevokePrivakeKey,
            string signedCommitment)
        {
            var assetModel = await _tradableAssets.GetItemAsync(offchainTransfer.AssetId);

            var result = assetModel.IsTrusted
                                    ? GetFakeResultForTrustedAsset()
                                    : await _bitcoinApiClient.Finalize(new FinalizeData
                                    {
                                        ClientPubKey = credentials.PublicKey,
                                        AssetId = offchainTransfer.AssetId,
                                        ClientRevokePubKey = revokePubKey,
                                        SignedByClientHubCommitment = signedCommitment,
                                        ExternalTransferId = offchainTransfer.ExternalTransferId,
                                        OffchainTransferId = offchainTransfer.Id
                                    });

            await _offchainEncryptedKeysRepository.UpdateKey(credentials.ClientId, offchainTransfer.AssetId, encryptedRevokePrivakeKey);

            if (result.HasError)
            {
                var required = offchainTransfer.Type == OffchainTransferType.FromHub || offchainTransfer.Type == OffchainTransferType.CashinToClient;
                return await InternalErrorProcessing(nameof(ProcessTransfer), result.Error, credentials, offchainTransfer, required);
            }

            await _offchainTransferRepository.CompleteTransfer(offchainTransfer.Id, blockchainHash: result.TxHash);

            await _offchainFinalizeCommandProducer.ProduceFinalize(offchainTransfer.Id, credentials.ClientId, result.TxHash);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = "0x0", //result.Transaction,
                OperationResult = OffchainOperationResult.ClientCommitment
            };
        }

        private async Task<OffchainResult> ProcessCashout(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, string revokePubKey, string encryptedRevokePrivakeKey,
            string signedCommitment)
        {
            var assetModel = await _tradableAssets.GetItemAsync(offchainTransfer.AssetId);

            var result = assetModel.IsTrusted
                                    ? GetFakeResultForTrustedAsset()
                                    : await _bitcoinApiClient.Finalize(new FinalizeData
                                    {
                                        ClientPubKey = credentials.PublicKey,
                                        AssetId = offchainTransfer.AssetId,
                                        ClientRevokePubKey = revokePubKey,
                                        SignedByClientHubCommitment = signedCommitment,
                                        ExternalTransferId = offchainTransfer.ExternalTransferId,
                                        OffchainTransferId = offchainTransfer.Id
                                    });

            await _offchainEncryptedKeysRepository.UpdateKey(credentials.ClientId, offchainTransfer.AssetId, encryptedRevokePrivakeKey);

            if (result.HasError)
                return await InternalErrorProcessing(nameof(ProcessCashout), result.Error, credentials, offchainTransfer, false);

            var isOnchain = offchainTransfer.Type == OffchainTransferType.ClientCashout || offchainTransfer.Type == OffchainTransferType.HubCashout;

            await _offchainTransferRepository.CompleteTransfer(offchainTransfer.Id, isOnchain, blockchainHash: result.TxHash);

            await _offchainFinalizeCommandProducer.ProduceFinalize(offchainTransfer.Id, credentials.ClientId, result.TxHash);

            return new OffchainResult
            {
                TransferId = offchainTransfer.Id,
                TransactionHex = "0x0", //result.Transaction,
                OperationResult = OffchainOperationResult.ClientCommitment
            };
        }

        private async Task<OffchainResult> ProcessChannelClosing(IWalletCredentials credentials, IOffchainTransfer offchainTransfer, string signedTransaction)
        {
            if (offchainTransfer.Completed || offchainTransfer.ClientId != credentials.ClientId)
                throw new OffchainException(ErrorCode.Exception, offchainTransfer.AssetId);

            var assetModel = await _tradableAssets.GetItemAsync(offchainTransfer.AssetId);

            var result = assetModel.IsTrusted && offchainTransfer.Type != OffchainTransferType.TrustedCashout
                ? GetFakeResultForTrustedAsset()
                : await _bitcoinApiClient.CloseChannel(new CloseChannelData
                {
                    ClientPubKey = credentials.PublicKey,
                    AssetId = offchainTransfer.AssetId,
                    SignedClosingTransaction = signedTransaction,
                    OffchainTransferId = offchainTransfer.Id
                });

            if (result.HasError)
                return await InternalErrorProcessing(nameof(ProcessChannelClosing), result.Error, credentials, offchainTransfer, false);

            await _offchainTransferRepository.CompleteTransfer(offchainTransfer.Id, true, blockchainHash: result.TxHash);

            await _offchainFinalizeCommandProducer.ProduceFinalize(offchainTransfer.Id, credentials.ClientId, result.TxHash);

            return new OffchainResult();
        }

        private async Task<OffchainResult> InternalErrorProcessing(string component, ErrorResponse error, IWalletCredentials credentials, IOffchainTransfer offchainTransfer, bool required)
        {
            if (error.ErrorCode == ErrorCode.ShouldOpenNewChannel)
                return await CreateChannel(credentials, offchainTransfer, required);

            await _logger.WriteWarningAsync("OffchainService", component, $"Client: [{credentials.ClientId}], error: [{error.ErrorCode}], transfer: [{offchainTransfer.Id}]");

            throw new OffchainException(error.ErrorCode, offchainTransfer.AssetId);
        }

        private async Task<decimal> GetExchangeAmount(string assetFrom, string assetTo, decimal amount, OrderAction action)
        {
            try
            {
                IEnumerable<ConversionResult> result = await _rateCalculatorClient.GetMarketAmountInBaseAsync(new List<AssetWithAmount>() { new AssetWithAmount { AssetId = assetFrom, Amount = (double)amount } }, assetTo, action.ToRateCalculatorDomain());

                var item = result.FirstOrDefault();

                if (item != null && item.Result == OperationResult.Ok)
                    return (decimal)(item.To?.Amount ?? 0);

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task CheckOrderPrice(string assetPair, OrderAction action, decimal price)
        {
            var isBuy = action == OrderAction.Buy;
            var orderBook = await _orderBooksService.GetAsync(assetPair);
            var deviation = _exchangeSettings.MaxLimitOrderDeviationPercent / 100;
            var bestPrice = 0.0;

            async Task CheckAndThrow()
            {
                if (isBuy && (decimal)bestPrice * (1 - deviation) > price ||
                    !isBuy && (decimal)bestPrice * (1 + deviation) < price)
                {
                    var buy = orderBook.First(x => x.IsBuy).Prices.OrderByDescending(x => x.Price).Take(5).ToList();
                    var sell = orderBook.First(x => !x.IsBuy).Prices.OrderBy(x => x.Price).Take(5).ToList();

                    await _logger.WriteWarningAsync(nameof(OffchainService), nameof(CheckOrderPrice),
                        $"Pair: {assetPair}, action: {action}, userPrice: {price}, orderBook: [buy - {buy.ToJson()}], [sell - {sell.ToJson()}]",
                        "Price gap error");

                    throw new TradeException(TradeExceptionType.PriceGapTooHigh);
                }
            }


            var straight = orderBook.First(x => x.IsBuy == isBuy);
            straight.Order();

            if (straight.Prices.Count > 0)
                bestPrice = straight.Prices[0].Price;

            if (bestPrice > 0)
            {
                await CheckAndThrow();
                return;
            }

            var reversed = orderBook.First(x => x.IsBuy == !isBuy);
            reversed.Order();

            if (reversed.Prices.Count > 0)
                bestPrice = reversed.Prices[0].Price;

            if (bestPrice > 0)
            {
                await CheckAndThrow();
            }
        }

        private OffchainResponse GetFakeResultForTrustedAsset()
        {
            return new OffchainResponse
            {
                Transaction = "010000000101000000000000000000000000000000000000000000000000000000000000000000000000ffffffff0100e1f505000000001976a9149aebf004e28552ad8942bcd3f99b27a4c95ecf5688ac00000000"
            };
        }

        private OffchainClosingResponse GetFakeClosingResultForTrustedAsset()
        {
            return new OffchainClosingResponse
            {
                Transaction = "010000000101000000000000000000000000000000000000000000000000000000000000000000000000ffffffff0100e1f505000000001976a9149aebf004e28552ad8942bcd3f99b27a4c95ecf5688ac00000000",
                ChannelClosing = true
            };
        }
    }
}
