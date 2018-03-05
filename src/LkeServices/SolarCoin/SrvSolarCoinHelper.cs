using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.BitCoin;
using Core.Settings;
using Core.Settings.LocalClients;
using Core.SolarCoin;
using LkeServices.Http;
using LkeServices.Notifications;

namespace LkeServices.SolarCoin
{
    public class SrvSolarCoinHelper : ISrvSolarCoinHelper
    {
        private readonly SolarCoinServiceClientSettings _solarCoinSettings;
        private readonly ILog _log;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly ISrvSlackNotifications _srvSlackNotifications;
        private readonly ISrvSolarCoinCommandProducer _solarCoinCommandProducer;

        public SrvSolarCoinHelper(SolarCoinServiceClientSettings solarCoinSettings, ILog log,
            IWalletCredentialsRepository walletCredentialsRepository, ISrvSlackNotifications srvSlackNotifications,
            ISrvSolarCoinCommandProducer solarCoinCommandProducer)
        {
            _solarCoinSettings = solarCoinSettings;
            _log = log;
            _walletCredentialsRepository = walletCredentialsRepository;
            _srvSlackNotifications = srvSlackNotifications;
            _solarCoinCommandProducer = solarCoinCommandProducer;
        }

        public async Task<string> SetNewSolarCoinAddress(IWalletCredentials walletCredentials)
        {
            try
            {
                var address =
                    (await new HttpRequestClient().GetRequest(_solarCoinSettings.ServiceUrl))
                        .DeserializeJson<GetAddressModel>().Address;

                await _walletCredentialsRepository.SetSolarCoinWallet(walletCredentials.ClientId, address);

                return address;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("SolarCoin", "GetAddress", "", ex);

                var msg = $"SolarCoin address was not set for {walletCredentials?.ClientId}.\n{ex.Message}";
                await _srvSlackNotifications.SendNotification(ChannelTypes.Errors, msg, "lykkeapi");
            }

            return null;
        }

        public Task SendCashOutRequest(string id, SolarCoinAddress addressTo, double amount)
        {
            return _solarCoinCommandProducer.ProduceCashOutCommand(id, addressTo, amount);
        }
    }

    #region Response Models

    public class GetAddressModel
    {
        public string Address { get; set; }
    }

    #endregion
}
