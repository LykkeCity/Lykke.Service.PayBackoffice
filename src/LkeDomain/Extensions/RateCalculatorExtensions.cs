using System.Collections.Generic;
using System.Linq;
using Core.Bitcoin;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.RateCalculator.Client.AutorestClient.Models;
using BalanceRecord = Lykke.Service.RateCalculator.Client.AutorestClient.Models.BalanceRecord;

namespace LkeDomain.Extensions
{
    public static class RateCalculatorExtensions
    {
        public static OrderAction ToRateCalculatorDomain(this Core.Exchange.OrderAction action)
        {
            return action == Core.Exchange.OrderAction.Buy ? OrderAction.Buy : OrderAction.Sell;
        }

        public static IEnumerable<BalanceRecord> ToRateCalculatorDomain(this IEnumerable<IBalanceRecord> balances)
        {
            return balances.Select(item => new BalanceRecord(item.Balance, item.AssetId));
        }

        public static MarketProfile ToRateCalculatorDomain(this IList<AssetPairModel> profile)
        {
            return new MarketProfile
            {
                Profile = profile.Select(item =>
                {
                    return new FeedData
                    {
                        DateTime = item.BidPriceTimestamp > item.AskPriceTimestamp
                            ? item.BidPriceTimestamp
                            : item.AskPriceTimestamp,
                        Asset = item.AssetPair,
                        Ask = item.AskPrice,
                        Bid = item.BidPrice
                    };
                }).ToList()
            };
        }
    }
}
