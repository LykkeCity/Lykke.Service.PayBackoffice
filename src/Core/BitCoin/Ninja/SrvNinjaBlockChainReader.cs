using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Assets;
using Core.Bitcoin;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.Assets.Client.Models.Extensions;

namespace Core.BitCoin.Ninja
{
    public class SrvNinjaBlockChainReader : ISrvBlockchainReader
    {
        private readonly string _url;
        private readonly Regex _addressRegex = new Regex("^[A-Za-z0-9]*$");

        public SrvNinjaBlockChainReader(string url)
        {
            if (string.IsNullOrEmpty(url))
                _url = "/";
            else
                _url = url[url.Length - 1] == '/' ? url : url + '/';
        }

        private static async Task<string> DoRequest(string url)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            var webResponse = await webRequest.GetResponseAsync();
            using (var receiveStream = webResponse.GetResponseStream())
            {
                using (var sr = new StreamReader(receiveStream))
                {
                    return await sr.ReadToEndAsync();
                }

            }
        }

        private async Task<T> DoRequest<T>(string url)
        {
            var result = await DoRequest(url);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
        }

        public async Task<IEnumerable<IObsoleteBlockchainTransaction>> ObsoleteGetTxsByAddressAsync(string address)
        {
            var data = await DoRequest<BtcAddressModel>($"{_url}balances/{address}?colored=true");

            var result = new List<IObsoleteBlockchainTransaction>();

            foreach (var item in data.Operations)
            {
                var tx = item.ObsoleteConvertToBlockchainTransaction(address);
                if (tx != null)
                    result.Add(tx);
            }

            return result;
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetBalanceChangesByAddressAsync(string address, int? until = null)
        {
            var untilParameter = until != null ? $"&until={until}" : "";
            var data = await DoRequest<BtcAddressModel>($"{_url}balances/{address}?colored=true&from=tip{untilParameter}");

            var result = new List<BlockchainTransaction>();

            foreach (var item in data.Operations)
            {
                result.Add(item.ConvertToBlockchainTransaction(address));
            }

            return result;
        }

        public async Task<IObsoleteBlockchainTransaction> GetByHashAsync(string hash)
        {

            try
            {
                var result = await DoRequest(_url + $"transactions/{hash}?colored=true");

                var contract = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionContract>(result);

                return contract.ObsoleteConvertToBlockchainTransaction();

            }
            catch (Exception)
            {

                return null;
            }

        }

        public async Task<string> GetColoredMultisigAsync(string multisig)
        {
            try
            {
                var result = await DoRequest<WhatIsItContract>(_url + "whatisit/" + multisig);

                return result.ColoredAddress;
            }
            catch (Exception)
            {

                throw new Exception("Invalid Multisig: " + multisig);
            }
        }

        public async Task<bool> IsColoredAddress(string address)
        {
            var result = await DoRequest<WhatIsItContract>(_url + "whatisit/" + address);
            return result.Type == WhatIsItTypes.COLORED_ADDRESS;
        }

        public async Task<bool> IsValidAddress(string address, bool enableColored = false)
        {
            try
            {
                if (!_addressRegex.IsMatch(address))
                    return false;

                var result = await DoRequest<WhatIsItContract>(_url + "whatisit/" + address);

                bool valid = false;
                if (result != null)
                {
                    valid = result.Type == WhatIsItTypes.PUBKEY_ADDRESS || result.Type == WhatIsItTypes.WITNESS_SCRIPT ||
                            (result.Type == WhatIsItTypes.SCRIPT_ADDRESS && result.IsP2SH);
                    valid = enableColored ? valid || result.Type == WhatIsItTypes.COLORED_ADDRESS : valid;
                }

                return valid;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> GetUncoloredAdress(string address)
        {
            var result = await DoRequest<WhatIsItContract>(_url + "whatisit/" + address);

            if (result.Type == WhatIsItTypes.COLORED_ADDRESS)
                return result.UncoloredAddress;

            return address;
        }

        public async Task<IEnumerable<IBalanceRecord>> GetBalancesForAdress(string address, Asset[] assets, bool onlySpendableCoins = false)
        {
            var result = await DoRequest($"{_url}balances/{address}/summary?colored=true");

            var contract = Newtonsoft.Json.JsonConvert.DeserializeObject<BalanceSummaryModel>(result);

            List<BalanceRecord> balances = new List<BalanceRecord>();
            var data = contract.Spendable;
            if (onlySpendableCoins)
            {
                data.Amount = Math.Min(contract.Spendable.Amount, contract.Confirmed.Amount);
                var intersectAssets = contract.Spendable.Assets.Select(o => o.AssetId).Intersect(contract.Confirmed.Assets.Select(o => o.AssetId));
                var spendableQuantities = contract.Spendable.Assets.ToDictionary(o => o.AssetId, o => o.Quantity);
                var confirmedQuantities = contract.Confirmed.Assets.ToDictionary(o => o.AssetId, o => o.Quantity);
                data.Assets = intersectAssets.Select(o => new BalanceSummaryModel.ColoredSummaryData
                {
                    AssetId = o,
                    Quantity = Math.Min(spendableQuantities[o], confirmedQuantities[o])
                }).ToArray();
            }
            var btc = assets.FirstOrDefault(x => x.Id == LykkeConstants.BitcoinAssetId);
            if (btc != null)
            {
                balances.Add(new BalanceRecord
                {
                    AssetId = LykkeConstants.BitcoinAssetId,
                    Balance = data.Amount * btc.Multiplier()
                });
            }

            var assetsDict =
                assets.Where(x => !string.IsNullOrEmpty(x.BlockChainAssetId)).ToDictionary(x => x.BlockChainAssetId);

            balances.AddRange(data.Assets
                .Where(x => assetsDict.ContainsKey(x.AssetId))
                .Select(asset => new BalanceRecord
                {
                    AssetId = assetsDict[asset.AssetId].Id,
                    Balance = asset.Quantity * assetsDict[asset.AssetId].Multiplier()
                }));

            return balances;
        }

        public async Task<double> GetBalancesSum(IEnumerable<string> addresses, IEnumerable<Asset> assets, Asset asset)
        {
            var getBalanceTasks = addresses.Select(address => GetBalance(address, assets, asset)).ToList();

            var balances = await Task.WhenAll(getBalanceTasks);

            return balances.Sum();
        }

        public async Task<int?> GetConfirmationsCount(string hash)
        {
            try
            {
                var result = await DoRequest(_url + $"transactions/{hash}?colored=true");

                var contract = Newtonsoft.Json.JsonConvert.DeserializeObject<TransactionContract>(result);

                return contract.Block.Confirmations;
            }
            catch (Exception)
            {

                return null;
            }
        }

        private async Task<double> GetBalance(string address, IEnumerable<Asset> assets, Asset asset)
        {
            var btc = assets.First(x => x.Id == LykkeConstants.BitcoinAssetId);

            var result = await DoRequest($"{_url}balances/{address}/summary?colored=true");
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<BalanceSummaryModel>(result);

            if (asset.BlockChainAssetId == null)
            {
                return model.Spendable.Amount * btc.Multiplier();
            }
            var balanceSummaryForAsset = model.Spendable.Assets.FirstOrDefault(x => x.AssetId == asset.BlockChainAssetId);

            if (balanceSummaryForAsset != null)
            {
                return balanceSummaryForAsset.Quantity * asset.Multiplier();
            }

            return 0;
        }

        public async Task<int> GetCurrentBlockHeight()
        {
            var result = await DoRequest($"{_url}blocks/tip?headerOnly=true");
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<BlockModel>(result);

            return model.AdditionalInformation.Height;
        }
    }
}
