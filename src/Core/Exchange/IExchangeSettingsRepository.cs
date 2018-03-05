using System.Threading.Tasks;

namespace Core.Exchange
{
    public interface IExchangeSettings
    {
        string BaseAssetIos { get; }
        string BaseAssetOther { get; }
        bool SignOrder { get; }
    }


    public class ExchangeSettings : IExchangeSettings
    {
        public string BaseAssetIos { get; set; }
        public string BaseAssetOther { get; set; }
        public bool SignOrder { get; set; }


        public static ExchangeSettings CreateDeafault()
        {
            return new ExchangeSettings
            {
                BaseAssetIos = string.Empty,
                BaseAssetOther = string.Empty,
                SignOrder = true
            };
        }
    }


    public interface IExchangeSettingsRepository
    {
        Task UpdateBaseAssetIosAsync(string clientId, string baseAsset);

        Task UpdateBaseAssetOtherAsync(string clientId, string baseAsset);

        Task UpdateSignOrderAsync(string clientId, bool value);

        Task<IExchangeSettings> GetAsync(string clientId);
    }


    public static class ExchangeSettingsRepositoryExt
    {
        private static IExchangeSettings _defaultExchangeSettings;

        public static async Task<IExchangeSettings> GetOrDefaultAsync(this IExchangeSettingsRepository exchangeSettingsRepository, string clientId)
        {
            var result = await exchangeSettingsRepository.GetAsync(clientId);
            if (result != null)
                return result;

            return _defaultExchangeSettings ?? (_defaultExchangeSettings = ExchangeSettings.CreateDeafault());
        }

        public static string BaseAsset(this IExchangeSettings settings, bool isIosDevice)
        {
            return isIosDevice ? settings.BaseAssetIos : settings.BaseAssetOther;
        }
    }

}
