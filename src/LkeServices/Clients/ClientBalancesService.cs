using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Accounts.PrivateWallets;
using Core.Bitcoin;
using Core.BitCoin;
using Core.Clients;
using Lykke.Service.RateCalculator.Client;
using Core.Ethereum;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.Balances.Client;

namespace LkeServices.Clients
{
    public class ClientBalancesService : IClientBalancesService
    {
        private readonly ISrvBlockchainReader _srvBlockchainReader;
        private readonly IBalancesClient _balancesClient;
        private readonly IRateCalculatorClient _rateCalculatorClient;
        private readonly IPrivateWalletsRepository _privateWalletsRepository;
        private readonly CachedTradableAssetsDictionary _tradableAssetsDict;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly ISrvEthereumHelper _srvEthereumHelper;

        public ClientBalancesService(ISrvBlockchainReader srvBlockchainReader,
            IBalancesClient balancesClient, IRateCalculatorClient rateCalculatorClient,
            IPrivateWalletsRepository privateWalletsRepository,
            CachedTradableAssetsDictionary tradableAssetsDict,
            IWalletCredentialsRepository walletCredentialsRepository,
            ISrvEthereumHelper srvEthereumHelper)
        {
            _srvBlockchainReader = srvBlockchainReader;
            _balancesClient = balancesClient;
            _rateCalculatorClient = rateCalculatorClient;
            _privateWalletsRepository = privateWalletsRepository;
            _tradableAssetsDict = tradableAssetsDict;
            _walletCredentialsRepository = walletCredentialsRepository;
            _srvEthereumHelper = srvEthereumHelper;
        }

        public async Task<ClientBalance> GetTotalTradingBalance(string clientId, string baseAssetId)
        {
            var clientBalance = new ClientBalance();

            var wallets = (await _balancesClient.GetClientBalances(clientId)).ToArray();

            if (wallets.Any(x => x.Balance > 0))
            {
                var toBase =
                    await _rateCalculatorClient.GetAmountInBaseAsync(
                        wallets.Where(x => x.Balance > 0).Select(x => new Lykke.Service.RateCalculator.Client.AutorestClient.Models.BalanceRecord((double)x.Balance, x.AssetId)), baseAssetId);


                clientBalance.TradingBalance = toBase.Sum(x => x.Balance);
            }

            return clientBalance;
        }

        public async Task<double> GetTotalPrivateWalletsBalance(string clientId, string baseAssetId)
        {
            var walletCreds = await _walletCredentialsRepository.GetAsync(clientId);

            var privateWallets = await _privateWalletsRepository.GetAllPrivateWallets(clientId, walletCreds);
            var addressesBtc = privateWallets.Where(y => y.BlockchainType == Blockchain.Bitcoin).Select(x => x.WalletAddress);
            var addressesEth = privateWallets.Where(y => y.BlockchainType == Blockchain.Ethereum).Select(x => x.WalletAddress);
            var assets = (await _tradableAssetsDict.Values()).ToArray();

            var balances = new List<IBalanceRecord>();
            var bitcoinAssets = assets.Where(x => x.Blockchain == Blockchain.Bitcoin);
            var ethereumAssets = assets.Where(x => x.Blockchain == Blockchain.Ethereum);
            foreach (var address in addressesBtc)
            {
                balances.AddRange(await _srvBlockchainReader.GetBalancesForAdress(address, bitcoinAssets?.ToArray(), true));
            }

            foreach (var address in addressesEth)
            {
                var ethBalance = await _srvEthereumHelper.GetWalletBalance(address, ethereumAssets);
                if (ethBalance != null && !ethBalance.HasError
                    && ethBalance.Result != null
                    && ethBalance.Result.Balances?.Count() != 0)
                {
                    balances.AddRange(ethBalance.Result.Balances);
                }
            }

            if (balances.Any(x => x.Balance > 0))
            {
                var grouped = balances.GroupBy(x => x.AssetId).Select(x => new
                {
                    AssetId = x.Key,
                    Balance = x.Sum(y => y.Balance)
                });

                var toBase =
                    await _rateCalculatorClient.GetAmountInBaseAsync(
                        grouped.Where(x => x.Balance > 0).Select(x => new Lykke.Service.RateCalculator.Client.AutorestClient.Models.BalanceRecord(x.Balance, x.AssetId)),
                        baseAssetId);

                return toBase.Sum(x => x.Balance);
            }

            return 0;
        }
    }
}
