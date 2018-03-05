using System;
using System.Threading.Tasks;
using Common.Log;
using Core.BitCoin;
using Core.Quanta;
using Core.Settings;
using LkeServices.Generated.QuantaApi;
using LkeServices.Generated.QuantaApi.Models;
using LkeServices.Notifications;

namespace LkeServices.Quanta
{
    public class QuantaService : IQuantaService
    {
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly QuantaServiceClientSettings _settings;
        private readonly ILog _log;
        private readonly ISrvSlackNotifications _srvSlackNotifications;
        private readonly IQuantaCommandProducer _quantaCommandProducer;

        public QuantaService(IWalletCredentialsRepository walletCredentialsRepository,
            QuantaServiceClientSettings settings, ILog log, ISrvSlackNotifications srvSlackNotifications,
            IQuantaCommandProducer quantaCommandProducer)
        {
            _walletCredentialsRepository = walletCredentialsRepository;
            _settings = settings;
            _log = log;
            _srvSlackNotifications = srvSlackNotifications;
            _quantaCommandProducer = quantaCommandProducer;
        }

        private QuantaApiClient Api => new QuantaApiClient(new Uri(_settings.ServiceUrl));

        public async Task<string> SetNewQuantaContract(IWalletCredentials walletCredentials)
        {
            try
            {
                var contract = (await Api.ApiClientRegisterGetAsync()) as RegisterResponse;

                if (contract != null)
                    await _walletCredentialsRepository.SetQuantaContract(walletCredentials.ClientId, contract.Contract);

                return contract?.Contract;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(QuantaService), nameof(SetNewQuantaContract), "", ex);

                var msg = $"Quanta contract was not set for {walletCredentials.ClientId}.\n{ex.Message}";
                await _srvSlackNotifications.SendNotification(ChannelTypes.Errors, msg, "lykkeapi");
            }

            return null;
        }

        public Task SendCashOutRequest(string id, string addressTo, double amount)
        {
            return _quantaCommandProducer.ProduceCashOutCommand(id, addressTo, amount);
        }

        public async Task<bool> IsQuantaUser(string address)
        {
            var isQuantaUser = (await Api.ApiClientIsQuantaUserGetAsync(address)) as IsQuantaUserResponse;

            return isQuantaUser?.IsQuantaUser ?? false;
        }
    }
}
