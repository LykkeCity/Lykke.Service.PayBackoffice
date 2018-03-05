using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Common;
using Core;
using Core.Exchange;
using Core.Notifications;
using Core.Offchain;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using OrderStatus = Core.Exchange.OrderStatus;

namespace LkeServices.Exchange
{
    public class LimitOrderService : ILimitOrderService
    {
        private readonly ILimitOrdersRepository _limitOrdersRepository;
        private readonly CachedAssetsDictionary _assets;
        private readonly CachedDataDictionary<string, AssetPair> _assetsPairsDict;
        private readonly IClientAccountClient _clientAccountService;
        private readonly IAppNotifications _appNotifications;

        public LimitOrderService(ILimitOrdersRepository limitOrdersRepository, CachedAssetsDictionary assets, CachedDataDictionary<string, AssetPair> assetsPairsDict, IClientAccountClient clientAccountService, IAppNotifications appNotifications)
        {
            _limitOrdersRepository = limitOrdersRepository;
            _assets = assets;
            _assetsPairsDict = assetsPairsDict;
            _clientAccountService = clientAccountService;
            _appNotifications = appNotifications;
        }

        public async Task CreateAsync(string pushTemplate, string id, string clientId, string assetPairId, double volume, double price,
            double remainigVolume)
        {
            await _limitOrdersRepository.CreateAsync(LimitOrder.Create(id, clientId, assetPairId, volume, price, volume));

            var type = volume > 0 ? OrderType.Buy : OrderType.Sell;

            var pair = await _assetsPairsDict.GetItemAsync(assetPairId);

            var priceAsset = await _assets.GetItemAsync(pair.QuotingAssetId);

            await SendPush(clientId, string.Format(pushTemplate, type.ToString().ToLower(), assetPairId, GetFixedString(Math.Abs(volume), pair.InvertedAccuracy), price, priceAsset.DisplayId), type, LimitOrderStatus.InOrderBook.ToString());
        }

        public async Task CancelAsync(string pushTemplate, string id)
        {
            var order = await _limitOrdersRepository.GetOrderAsync(id);

            var cancelled = LimitOrderStatus.Cancelled.ToString();

            if (order.Status == cancelled)
                return;

            await _limitOrdersRepository.FinishAsync(order, cancelled);

            var type = order.Volume > 0 ? OrderType.Buy : OrderType.Sell;

            var pair = await _assetsPairsDict.GetItemAsync(order.AssetPairId);

            var priceAsset = await _assets.GetItemAsync(pair.QuotingAssetId);

            await SendPush(order.ClientId, string.Format(pushTemplate, type.ToString().ToLower(), order.AssetPairId, GetFixedString(Math.Abs(order.RemainingVolume), pair.InvertedAccuracy), order.Price, priceAsset.DisplayId), type, LimitOrderStatus.Cancelled.ToString());
        }

        public async Task RejectAsync(string pushTemplate, string id, string clientId, string assetPairId, double volume, double price, string status)
        {
            var order = await _limitOrdersRepository.GetOrderAsync(id);

            await _limitOrdersRepository.FinishAsync(order, status);

            var type = volume > 0 ? OrderType.Buy : OrderType.Sell;

            var pair = await _assetsPairsDict.GetItemAsync(assetPairId);

            var priceAsset = await _assets.GetItemAsync(pair.QuotingAssetId);

            await SendPush(clientId, string.Format(pushTemplate, type.ToString().ToLower(), assetPairId, GetFixedString(Math.Abs(volume), pair.InvertedAccuracy), price, priceAsset.DisplayId), type, status);
        }

        public Task RemoveAsync(string id, string clientId)
        {
            return _limitOrdersRepository.RemoveAsync(id, clientId);
        }

        private async Task SendPush(string clientId, string text, OrderType orderType, string status)
        {
            var pushSettings = await _clientAccountService.GetPushNotificationAsync(clientId);
            if (pushSettings.Enabled)
            {
                var clientAcc = await _clientAccountService.GetByIdAsync(clientId);

                await _appNotifications.SendLimitOrderNotification(new[] { clientAcc.NotificationsId }, text, orderType, status);
            }
        }

        private static string GetFixedString(double value, int accuracy)
        {
            var format = string.Format("0.{0}", new string('#', accuracy));
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
