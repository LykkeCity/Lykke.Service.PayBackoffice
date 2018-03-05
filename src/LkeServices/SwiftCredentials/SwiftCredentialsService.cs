using System;
using System.Threading.Tasks;
using Common;
using Core;
using Core.Assets;
using Core.Clients;
using Core.Regulator;
using Core.SwiftCredentials;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Lykke.Service.Assets.Client.Models;

namespace LkeServices.SwiftCredentials
{
    public class SwiftCredentialsService : ISwiftCredentialsService
    {
        private readonly IPersonalDataService _personalDataService;
        private readonly IRegulatorRepository _regulatorRepository;
        private readonly ISwiftCredentialsRepository _swiftCredentialsRepository;
        private readonly CachedTradableAssetsDictionary _tradableAssetsDictionary;

        public SwiftCredentialsService(IPersonalDataService personalDataService,
            IRegulatorRepository regulatorRepository,
            ISwiftCredentialsRepository swiftCredentialsRepository,
            CachedTradableAssetsDictionary tradableAssetsDictionary)
        {
            _personalDataService = personalDataService;
            _regulatorRepository = regulatorRepository;
            _swiftCredentialsRepository = swiftCredentialsRepository;
            _tradableAssetsDictionary = tradableAssetsDictionary;
        }

        public async Task<ISwiftCredentials> GetDefaultCredentialsAsync(string assetId)
        {
            var regulatorId = (await _regulatorRepository.GetByIdOrDefaultAsync(null)).InternalId;

            //if no credentials, try to get default for regulator
            var credentials = await _swiftCredentialsRepository.GetCredentialsAsync(regulatorId, assetId) ??
                              await _swiftCredentialsRepository.GetCredentialsAsync(regulatorId);

            return await BuildCredentials(assetId, credentials, null);
        }

        public async Task<ISwiftCredentials> GetCredentialsAsync(string assetId, string clientId)
        {
            var personalData = await _personalDataService.GetAsync(clientId);

            if (personalData == null)
                throw new Exception($"Client {clientId} not found");

            return await GetCredentialsAsync(assetId, personalData.SpotRegulator, personalData.Email);
        }

        public async Task<ISwiftCredentials> GetCredentialsAsync(string assetId, string clientSpotRegulator, string clientEmail)
        {
            var regulatorId = clientSpotRegulator ??
                              (await _regulatorRepository.GetByIdOrDefaultAsync(null)).InternalId;

            //if no credentials, try to get default for regulator
            var credentials = await _swiftCredentialsRepository.GetCredentialsAsync(regulatorId, assetId) ??
                              await _swiftCredentialsRepository.GetCredentialsAsync(regulatorId);

            return await BuildCredentials(assetId, credentials, clientEmail);
        }

        private async Task<ISwiftCredentials> BuildCredentials(string assetId, ISwiftCredentials sourceCredentials, string clientEmail)
        {
            if (sourceCredentials == null)
                return null;

            var asset = await _tradableAssetsDictionary.GetItemAsync(assetId);
            var assetTitle = asset?.DisplayId ?? assetId;

            var clientIdentity = clientEmail != null ? clientEmail.Replace("@", ".") : "{1}";
            var purposeOfPayment = string.Format(sourceCredentials.PurposeOfPayment, assetTitle, clientIdentity);

            if (!purposeOfPayment.Contains(assetId) && !purposeOfPayment.Contains(assetTitle))
                purposeOfPayment += assetTitle;

            if (!purposeOfPayment.Contains(clientIdentity))
                purposeOfPayment += clientIdentity;

            return new Core.SwiftCredentials.SwiftCredentials
            {
                AssetId = assetId,
                RegulatorId = sourceCredentials.RegulatorId,
                BIC = sourceCredentials.BIC,
                PurposeOfPayment = purposeOfPayment,
                CompanyAddress = sourceCredentials.CompanyAddress,
                AccountNumber = sourceCredentials.AccountNumber,
                BankAddress = sourceCredentials.BankAddress,
                AccountName = sourceCredentials.AccountName,
                CorrespondentAccount = sourceCredentials.CorrespondentAccount
            };
        }
    }
}
