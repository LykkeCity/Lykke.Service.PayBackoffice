using System;
using Core.Exchange;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.Assets.Client.Models.Extensions;

namespace Core
{
    public static class FxUtils
    {
        public static double GetOrderVolume(double volume, double price, OrderAction orderAction, string baseAsset, AssetPair assetPair)
        {
            if (orderAction == OrderAction.Buy)
                return assetPair.BaseAssetId == baseAsset
                    ? volume / price
                    : volume * price;

            return volume;
        }


        public static bool PricesAreTheSame(this AssetPair assetPair, double price1, double price2)
        {
            var multiplier = Math.Pow(10, assetPair.Accuracy);
            var p1 = (long) Math.Round(price1 * multiplier);
            var p2 = (long) Math.Round(price2 * multiplier);

            return p1 == p2;
        }

        public static string PriceToStr(this double price, AssetPair assetPair)
        {
            var mask = "0." + new string('0', assetPair.Accuracy);
            return price.ToString(mask);
        }

        public static int PriceToInt(this double price, AssetPair assetPair)
        {
            return (int) Math.Round(price * assetPair.Multiplier());
        }

        public static string PriceToStr(this double? price, AssetPair assetPair)
        {
            return price?.PriceToStr(assetPair);
        }

        public static string MoneyToStr(this double money)
        {
            return money.ToString("0.00");
        }
    }
}
