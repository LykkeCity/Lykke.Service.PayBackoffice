using System;
using Core.Assets;
using Core.Exchange;
using Lykke.Service.Assets.Client.Models;
using NUnit.Framework;
using WalletApi.Models;

namespace WebApiTests.Converters
{
    [TestFixture]
    public class MarketOrderConverterTests
    {
        [Test]
        public void Btc_for_usd_buy_test()
        {
            var assetPair = new AssetPair
            {
                Accuracy = 3,
                InvertedAccuracy = 8
            };

            var marketOrder = new MarketOrder
            {
                OrderAction = OrderAction.Buy,
                Price = 512.3456789,
                Volume = 3,
                AssetPairId = "BTCUSD",
                Straight = true
            };

            var apiOrder = marketOrder.ConvertToApiModel(assetPair, 2);

            Assert.IsTrue(Math.Abs(512.345 - apiOrder.Price.Value) <= 1E-3);
            Assert.AreEqual(assetPair.Accuracy, apiOrder.Accuracy);
            Assert.IsTrue(Math.Abs(1537.04 - apiOrder.TotalCost) <= 1E-3);
        }

        [Test]
        public void Usd_for_btc_buy_test()
        {
            var assetPair = new AssetPair
            {
                Accuracy = 3,
                InvertedAccuracy = 8
            };

            var marketOrder = new MarketOrder()
            {
                OrderAction = OrderAction.Buy,
                Price = 512.3456789,
                Volume = 999,
                AssetPairId = "BTCUSD",
                Straight = false
            };

            var apiOrder = marketOrder.ConvertToApiModel(assetPair, 8);

            Assert.IsTrue(Math.Abs(0.00195181 - apiOrder.Price.Value) <= 1E-8);
            Assert.AreEqual(assetPair.Accuracy, apiOrder.Accuracy);
            Assert.IsTrue(Math.Abs(1.94985819 - apiOrder.TotalCost) <= 1E-8);
        }

        /// <summary>
        /// Selling BTC for USD
        /// </summary>
        [Test]
        public void Btc_for_usd_sell_test()
        {
            var assetPair = new AssetPair
            {
                Accuracy = 3,
                InvertedAccuracy = 8
            };

            var marketOrder = new MarketOrder()
            {
                OrderAction = OrderAction.Buy,
                Price = 512.3456789,
                Volume = -1.94984820,
                AssetPairId = "BTCUSD",
                Straight = true
            };

            var apiOrder = marketOrder.ConvertToApiModel(assetPair, 2);

            Assert.AreEqual(512.345, apiOrder.Price);
            Assert.AreEqual(assetPair.Accuracy, apiOrder.Accuracy);
            Assert.AreEqual(998.99, apiOrder.Volume);
        }

        [Test]
        public void Usd_for_btc_sell_test()
        {
            var assetPair = new AssetPair
            {
                Accuracy = 3,
                InvertedAccuracy = 8
            };

            var marketOrder = new MarketOrder()
            {
                OrderAction = OrderAction.Buy,
                Price = 512.3456789,
                Volume = -999,
                AssetPairId = "BTCUSD",
                Straight = false
            };

            var apiOrder = marketOrder.ConvertToApiModel(assetPair, 8);

            Assert.AreEqual(0.00195180, apiOrder.Price);
            Assert.AreEqual(assetPair.Accuracy, apiOrder.Accuracy);
            Assert.AreEqual(1.94984820, apiOrder.Volume);
        }

        [Test]
        public void Buy_chf_for_btc_round_to_upper_test()
        {
            var assetPair = new AssetPair
            {
                Accuracy = 3,
                InvertedAccuracy = 8
            };

            var marketOrder = new MarketOrder()
            {
                OrderAction = OrderAction.Buy,
                Price = 654.234,
                Volume = 0.03,
                AssetPairId = "BTCCHF",
                Straight = false
            };

            var apiOrder = marketOrder.ConvertToApiModel(assetPair, 8);

            Assert.IsTrue(Math.Abs(0.00152851 - apiOrder.Price.Value) <= 1E-8);
            Assert.AreEqual(assetPair.Accuracy, apiOrder.Accuracy);
            Assert.IsTrue(Math.Abs(0.03 - apiOrder.Volume) <= 1E-8);
            Assert.IsTrue(Math.Abs(0.00004586 - apiOrder.TotalCost) <= 1E-8);
        }
    }
}
