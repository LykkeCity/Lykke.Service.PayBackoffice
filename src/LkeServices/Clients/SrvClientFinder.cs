using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Accounts.PrivateWallets;
using Core.BitCoin;
using System.Collections.Generic;
using Core.Blockchain;
using Core.Extensions;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;

namespace LkeServices.Clients
{
    public class SrvClientFinder
    {
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientAccountClient _clientAccountService;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;
        private readonly IPrivateWalletsRepository _privateWalletsRepository;
        private readonly IBcnClientCredentialsRepository _bcnClientCredentialsRepository;

        public SrvClientFinder(IPersonalDataService personalDataService,
            IClientAccountClient clientAccountService,
            IWalletCredentialsRepository walletCredentialsRepository,
            IPrivateWalletsRepository privateWalletsRepository, IBcnClientCredentialsRepository bcnClientCredentialsRepository)
        {
            _personalDataService = personalDataService;
            _clientAccountService = clientAccountService;
            _walletCredentialsRepository = walletCredentialsRepository;
            _privateWalletsRepository = privateWalletsRepository;
            _bcnClientCredentialsRepository = bcnClientCredentialsRepository;
        }

        public async Task<IEnumerable<ClientAccountInformationModel>> FindClientAccountsAsync(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
                return null;

            return await _clientAccountService.GetClientsByEmailAsync(phrase);
        }

        public async Task<IPersonalData> FindClientAsync(string phrase)
        {
            if (string.IsNullOrEmpty(phrase))
                return null;

            if (phrase.IsGuid())
                return await _personalDataService.GetAsync(phrase);

            if (phrase.IsValidEmailAndRowKey())
            {
                var client = await _clientAccountService.GetClientsByEmailAsync(phrase);
                if (client == null)
                    return null;

                return await _personalDataService.GetAsync(client.FirstOrDefault()?.Id);
            }

            IPersonalData result = await _personalDataService.FindClientsByPhrase(phrase);

            if (result != null)
                return result;

            result = await FindByWalletAsync(phrase)
                ?? await FindByPrivateWalletAsync(phrase);

            return result;
        }

        public Task<ISearchPersonalData<PersonalDataModel>> FindClientsByPhrase(string phrase)
        {
            return _personalDataService.FindClientsByPhrase(phrase);
        }

        public async Task<IEnumerable<IPersonalData>> FindByPhoneNumber(string phoneNumber)
        {
            var clientIds = await _clientAccountService.GetClientsByPhoneAsync(phoneNumber);
            if (clientIds == null)
            {
                return null;
            }

            return await _personalDataService.GetAsync(clientIds);
        }

        private async Task<IPersonalData> FindByMultisigIndex(string phrase)
        {
            var clientId = await _walletCredentialsRepository.GetClientIdByMultisig(phrase);
            if (clientId == null)
                return null;
            return await _personalDataService.GetAsync(clientId);
        }

        private async Task<IPersonalData> FindByBcnCredentialsIndex(string phrase)
        {
            var bcnCredentialsRecord = await _bcnClientCredentialsRepository.GetByAssetAddressAsync(phrase);
            if (bcnCredentialsRecord?.ClientId == null)
                return null;
            return await _personalDataService.GetAsync(bcnCredentialsRecord.ClientId);
        }

        private async Task<IPersonalData> FindByWalletAsync(string phrase)
        {
            var pd = await FindByMultisigIndex(phrase) ?? await FindByBcnCredentialsIndex(phrase);

            if (pd != null)
                return pd;

            var result =
                await
                    _walletCredentialsRepository.ScanAndFind(
                        item => item.Address == phrase
                            || item.MultiSig == phrase
                            || item.ColoredMultiSig == phrase
                            || item.EthConversionWalletAddress == phrase
                            || item.SolarCoinWalletAddress == phrase
                            || item.ChronoBankContract == phrase
                            || item.QuantaContract == phrase);


            if (result == null)
                return null;

            return await _personalDataService.GetAsync(result.ClientId);
        }

        private async Task<ISearchPersonalData<IPersonalData>> FindByPrivateWalletAsync(string phrase)
        {
            var clientIds = (await _privateWalletsRepository
                .GetAllStoredWallets(phrase))
                .Select(x => x.ClientId)
                .ToList();

            if (clientIds == null || !clientIds.Any())
                return null;

            var records = (await _personalDataService
                .GetAsync(clientIds))
                .ToList();

            var result = SearchPersonalData.Create(records.First());
            result.OtherClients = records.Skip(1).ToList();
            return result;
        }
    }
}
