using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;

namespace Core.Accounts
{

    public interface IWallet
    {
        double Balance { get; }
        string AssetId { get; }
        double Reserved { get; }
    }

    public class Wallet : IWallet
    {
        public string AssetId { get; set; }
        public double Reserved { get; set; }
        public double Balance { get; set; }

        public static Wallet Create(Asset asset, double balance = 0)
        {
            return new Wallet
            {
                AssetId = asset.Id,
                Balance = balance
            };
        }
    }

    public interface IWalletsRepository
    {
        Task<IEnumerable<IWallet>> GetAsync(string clientId);
        Task<IWallet> GetAsync(string clientId, string assetId);
        Task UpdateBalanceAsync(string clientId, string assetId, double balance);
        Task<Dictionary<string, double>> GetTotalBalancesAsync();

        Task GetWalletsByChunkAsync(Func<IEnumerable<KeyValuePair<string, IEnumerable<IWallet>>>, Task> chunk);
    }


    public static class WalletsRespostoryExtention
    {
        public static async Task<double> GetWalletBalanceAsync(this IWalletsRepository walletsRepository, string clientId, string assetId)
        {
            var entity = await walletsRepository.GetAsync(clientId, assetId);
            if (entity == null)
                return 0;

            return entity.Balance;
        }

        public static async Task<IEnumerable<IWallet>> GetAsync(this IWalletsRepository walletsRepository, string clientId, IEnumerable<Asset> assets)
        {
            var wallets = await walletsRepository.GetAsync(clientId);


            return assets.Select(asset => wallets.FirstOrDefault(wallet => wallet.AssetId == asset.Id) ?? Wallet.Create(asset));
        }
    }
}
