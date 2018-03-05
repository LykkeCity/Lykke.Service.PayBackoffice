using System;
using System.Threading.Tasks;
using Core.BitCoin;
using Core.Settings;
using LkeServices.Generated.ClientSigningApi;
using LkeServices.Generated.ClientSigningApi.Models;

namespace LkeServices.Bitcoin
{
    public class ClientSigningService : IClientSigningService
    {
        private readonly string _clientSigningApiUrl;
        private LykkeSigningAPI Api => new LykkeSigningAPI(new Uri(_clientSigningApiUrl));

        public ClientSigningService(BitcoinCoreSettings bitcoinCoreSettings)
        {
            _clientSigningApiUrl = bitcoinCoreSettings.ClientSigningApiUrl;
        }

        public Task PushClientKey(string privateKey)
        {
            return Api.ApiBitcoinAddkeyPostAsync(new AddKeyRequest
            {
                Key = privateKey
            });
        }

        public async Task<string> SignTransaction(string transactionHex)
        {
            var transaction = await Api.ApiBitcoinSignPostAsync(new BitcoinTransactionSignRequest
            {
                Transaction = transactionHex
            });

            return transaction.SignedTransaction;
        }

        public async Task<string> GetPrivateKey(string address)
        {
            var key = await Api.ApiBitcoinGetkeyGetAsync(address);
            return key.PrivateKey;
        }
    }
}
