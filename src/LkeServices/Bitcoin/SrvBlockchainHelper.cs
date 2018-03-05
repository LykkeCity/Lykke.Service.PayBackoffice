using System;
using System.Threading.Tasks;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.Bitcoin.Api.Client.BitcoinApi.Models;
using Common;
using Common.Log;
using Core;
using Core.BitCoin;
using Core.Blockchain;
using Core.Settings;
using NBitcoin;
using Lykke.Service.ClientAccount.Client;

namespace LkeServices.Bitcoin
{
    public class SrvBlockchainHelper : ISrvBlockchainHelper
    {
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly ILog _log;
        private readonly IWalletCredentialsHistoryRepository _walletCredentialsHistoryRepository;
        private readonly IClientAccountClient _clientAccountService;
        private readonly IClientSigningService _clientSigningService;
        private readonly IBitcoinApiClient _bitcoinApiClient;
        private readonly IBcnClientCredentialsRepository _bcnClientCredentialsRepository;

        public SrvBlockchainHelper(IWalletCredentialsRepository walletCredentialsRepository,
            ILog log,
            IWalletCredentialsHistoryRepository walletCredentialsHistoryRepository,
            IClientAccountClient clientAccountService, IClientSigningService clientSigningService,
            IBitcoinApiClient bitcoinApiClient, IBcnClientCredentialsRepository bcnClientCredentialsRepository)
        {
            _walletCredentialsRepository = walletCredentialsRepository;
            _log = log;
            _walletCredentialsHistoryRepository = walletCredentialsHistoryRepository;
            _clientAccountService = clientAccountService;
            _clientSigningService = clientSigningService;
            _bitcoinApiClient = bitcoinApiClient;
            _bcnClientCredentialsRepository = bcnClientCredentialsRepository;
        }

        public async Task<IWalletCredentials> GenerateWallets(string clientId, string clientPubKeyHex, string encodedPrivateKey, string privateKey, NetworkType networkType)
        {
            var network = networkType == NetworkType.Main ? Network.Main : Network.TestNet;

            PubKey clientPubKey = new PubKey(clientPubKeyHex);
            var clientAddress = clientPubKey.GetAddress(network);
            var clientAddressWif = clientAddress.ToWif();
            var coloredAddressWif = clientAddress.ToColoredAddress().ToWif();

            var wallets = await GetWalletsForPubKey(clientPubKeyHex);

            var currentWalletCreds = await _walletCredentialsRepository.GetAsync(clientId);

            if (!string.IsNullOrWhiteSpace(privateKey))
                await _clientSigningService.PushClientKey(privateKey);

            IWalletCredentials walletCreds;
            if (currentWalletCreds == null)
            {
                var btcConvertionWallet = GetNewAddressAndPrivateKey(network);

                walletCreds = WalletCredentials.Create(
                    clientId, clientAddressWif, null, wallets.MultiSigAddress,
                    wallets.ColoredMultiSigAddress,
                    btcConvertionWallet.PrivateKey, btcConvertionWallet.Address, encodedPk: encodedPrivateKey,
                    pubKey: clientPubKeyHex);

                await Task.WhenAll(
                    _walletCredentialsRepository.SaveAsync(walletCreds),
                    _bcnClientCredentialsRepository.SaveAsync(BcnCredentialsRecord.Create(LykkeConstants.BitcoinAssetId, clientId, null, wallets.SegwitAddress, clientPubKeyHex))
                );
            }
            else
            {
                walletCreds = WalletCredentials.Create(
                    clientId, clientAddressWif, null, wallets.MultiSigAddress,
                    wallets.ColoredMultiSigAddress, null, null, encodedPk: encodedPrivateKey,
                    pubKey: clientPubKeyHex);

                if (await _bcnClientCredentialsRepository.GetAsync(clientId, LykkeConstants.BitcoinAssetId) == null)
                    await _bcnClientCredentialsRepository.SaveAsync(BcnCredentialsRecord.Create(
                        LykkeConstants.BitcoinAssetId, clientId, null, wallets.SegwitAddress,
                        clientPubKeyHex));

                await _walletCredentialsHistoryRepository.InsertHistoryRecord(currentWalletCreds);
                await _walletCredentialsRepository.MergeAsync(walletCreds);
            }

            await SetDefaultRefundAddress(clientId, coloredAddressWif);

            return walletCreds;
        }

        private async Task SetDefaultRefundAddress(string clientId, string coloredAddressWif)
        {
            var refundSettings = await _clientAccountService.GetRefundAddressAsync(clientId);
            if (string.IsNullOrEmpty(refundSettings.Address))
            {
                await _clientAccountService.SetRefundAddressAsync(clientId, coloredAddressWif,
                    LykkeConstants.DefaultRefundTimeoutDays, true);
            }
        }

        public async Task<ITransaction> GenerateTransferTransaction(string sourceAddress, string destAddress,
            double amount, string assetId)
        {
            var response = await _bitcoinApiClient.TransactionTransfer(null, sourceAddress, destAddress, (decimal)amount, assetId);

            if (response.Transaction != null)
                return new CreateUnsignedTransferResponse
                {
                    TransactionHex = response.Transaction.Transaction,
                    Id = response.Transaction.TransactionId.ToString()
                };

            await _log.WriteWarningAsync("SrvBlockchainHelper", "GenerateTransferTransaction", new
            {
                sourceAddress,
                destAddress,
                amount,
                assetId
            }.ToJson(), response.Error?.ToJson());

            if (response.HasError)
                throw new BitcoinApiException(new ErrorResponse { Code = response.Error.Code, Message = response.Error.Message });

            return null;
        }

        public async Task BroadcastTransaction(ITransaction transaction)
        {
            await _bitcoinApiClient.TransactionBroadcast(new Guid(transaction.Id), transaction.Hex);
        }

        public bool VerifyMessage(string pubKeyAddress, string message, string signedMessage)
        {
            var address = new BitcoinPubKeyAddress(pubKeyAddress);
            try
            {
                return address.VerifyMessage(message, signedMessage);
            }
            catch
            {
                return false;
            }
        }

        #region Tools

        private async Task<GetWalletResponse> GetWalletsForPubKey(string pubKeyHex)
        {
            try
            {
                var response = await _bitcoinApiClient.GetWallet(pubKeyHex);

                var segwitResponse = await _bitcoinApiClient.GetSegwitWallet(pubKeyHex);

                if (response.HasError || segwitResponse.HasError)
                    throw new Exception($"Bad response from Bitcoin, error: {response.Error.ToJson()}");

                return new GetWalletResponse
                {
                    ColoredMultiSigAddress = response.ColoredMultisig,
                    MultiSigAddress = response.Multisig,
                    SegwitAddress = segwitResponse.Address
                };
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SrvBlockchainHelper", "GenerateTransferTransaction", pubKeyHex, ex);
                throw;
            }
        }

        class WalletKeyAndAddress
        {
            public string PrivateKey { get; set; }
            public string Address { get; set; }
        }

        private WalletKeyAndAddress GetNewAddressAndPrivateKey(Network network)
        {
            Key key = new Key();
            BitcoinSecret secret = new BitcoinSecret(key, network);

            var walletAddress = secret.GetAddress().ToWif();
            var walletPrivateKey = secret.PrivateKey.GetWif(network).ToWif();

            return new WalletKeyAndAddress
            {
                Address = walletAddress,
                PrivateKey = walletPrivateKey
            };
        }

        #endregion

        #region WalletBackend response models

        internal class CreateUnsignedTransferResponse : ITransaction
        {
            public string Id { get; set; }
            public string Hex => TransactionHex;
            public string TransactionHex { get; set; }
        }

        internal class GetWalletResponse
        {
            public string MultiSigAddress { get; set; }
            public string ColoredMultiSigAddress { get; set; }
            public string SegwitAddress { get; set; }
        }

        #endregion
    }
}
