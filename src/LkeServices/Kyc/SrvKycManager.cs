using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Core.BackOffice;

using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;

using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.Kyc.Abstractions.Services;
using Core.Kyc;

using Flurl.Http;

using Lykke.Service.ClientAccount.Client;
using Lykke.Service.Kyc.Abstractions.Services.Models;
using Lykke.Service.JumioIntegration.Client.AutorestClient.Models;
using Lykke.Service.Kyc.Client;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LkeServices.Kyc
{
    public class SrvKycManager
    {
        private readonly IMenuBadgesRepository _menuBadgesRepository;
        private readonly IPersonalDataService _personalDataService;
        private readonly IClientAccountClient _clientAccountService;
        private readonly IKycStatusService _kycStatusService;
        private readonly IKycProfileService _profileService;
        private readonly IJumioService _jumioService;
        private readonly KycServiceSettings _kycServiceSettings;

        public SrvKycManager(
            IMenuBadgesRepository menuBadgesRepository,
            IPersonalDataService personalDataService,
            IClientAccountClient clientAccountService,
            IKycStatusService kycStatusService,
            IKycProfileService profileService,
            IJumioService jumioService,
            KycServiceSettings kycServiceSettings
        )
        {
            _menuBadgesRepository = menuBadgesRepository;
            _personalDataService = personalDataService;
            _clientAccountService = clientAccountService;
            _kycStatusService = kycStatusService;
            _profileService = profileService;
            _jumioService = jumioService;
            _kycServiceSettings = kycServiceSettings;
        }

        #region KYC Status
        public async Task UpdateKycBadge() {
            var counts = await _kycStatusService.GetCountsByStatusAsync(KycStatus.Pending, KycStatus.Complicated, KycStatus.ReviewDone, KycStatus.JumioInProgress, KycStatus.JumioFailed, KycStatus.JumioOk);
            var tasks = counts.Select(kycStatusCount => _menuBadgesRepository.SaveBadgeAsync($"{MenuBadges.Kyc}{kycStatusCount.Key}", kycStatusCount.Value.ToString()));
            await Task.WhenAll(tasks);
        }

        public async Task<bool> ChangeKycStatus(string clientId, KycStatus kycStatus, string changer, bool isBackoffice = false)
        {
            var changeResult = await _kycStatusService.ChangeKycStatusAsync(clientId, kycStatus, changer);
            if (!changeResult) return false;
            await UpdateKycBadge();

            if (kycStatus == KycStatus.Pending)
            {
                var jumioVerificationResult = await _jumioService.GetVerificationResult(clientId);
                if (jumioVerificationResult?.Status != VerificationStatus.Approved)
                {
                    var clientAccount = await _clientAccountService.GetByIdAsync(clientId);
                    if (clientAccount != null && clientAccount.PartnerId == null)
                        _jumioService.StartVerification(clientId);
                }
            }

            return true;
        }

        public async Task<IEnumerable<IPersonalData>> GetAccountsToCheck(KycStatus status)
        {
            var ids = (await _kycStatusService.GetClientsByStatusAsync(status))[status].ToList();
            var data = await _personalDataService.GetAsync(ids) ?? Enumerable.Empty<IPersonalData>();
            return data.OrderBy(x => ids.IndexOf(x.Id)).ToList();
        }

        public async Task<KycStatus> GetKycStatusAsync(string clientId)
        {
            return await _kycStatusService.GetKycStatusAsync(clientId);
        }

        #endregion

        #region Jumio

        public async Task<JumioFaceMatch?> GetFaceMatchAsync(string clientId)
        {
            return await _jumioService.GetFaceMatch(clientId);
        }

        #endregion

        #region Clients

        public async Task UpdatePersonalDataAsync(IPersonalData personalData, string changer)
        {
            var dataBefore = await _personalDataService.GetAsync(personalData.Id);
            var changes = KycPersonalDataChangesTmp.Create(changer, personalData, dataBefore);
            await GetClient($"id/{personalData.Id}/PersonalData").PutJsonAsync(changes);
        }

		#region Hotfix. Need to remove when release https://lykkex.atlassian.net/browse/LWDEV-5995
        private IFlurlClient GetClient(string action)
        {
            return $"{_kycServiceSettings.ServiceUri}/api/KycProfile/{action}".WithHeader("api-key", _kycServiceSettings.ApiKey);
        }

        public class KycPersonalDataChangesTmp
        {
            public string Changer { get; set; }

            public Dictionary<string, JToken> Items { get; set; }

            public static KycPersonalDataChangesTmp Create(string changer, object current, object original = null)
            {
                return new KycPersonalDataChangesTmp
                {
                    Changer = changer,
                    Items = DifferenceWith(current, original)
                };
            }

            private static Dictionary<string, JToken> DifferenceWith(object current, object original)
            {
                current = current ?? new { };
                original = original ?? new { };

                var currentJson = JsonConvert.SerializeObject(current);
                var originalJson = JsonConvert.SerializeObject(original);

                var currentObject = (JObject)JsonConvert.DeserializeObject(currentJson);
                var originalObject = (JObject)JsonConvert.DeserializeObject(originalJson);

                return currentObject.Properties()
                    .Where(x => JsonConvert.SerializeObject(originalObject[x.Name]) != JsonConvert.SerializeObject(x.Value))
                    .ToDictionary(x => x.Name, x => x.Value);
            }
        }
		#endregion

        public async Task ChangePhoneAsync(string clientId, string phoneNumber, string changer)
        {
            var changes = KycPersonalDataChanges.Create(nameof(IPersonalData.ContactPhone), phoneNumber, changer);
            await _profileService.UpdatePersonalDataAsync(clientId, changes);
        }

        public async Task ChangeFullNameAsync(string clientId, string fullName, string changer)
        {
            var fullNameSplit = fullName.Split(" ".ToCharArray());

            var firstName = (fullNameSplit[0] ?? "").Trim();
            var lastName = fullName.Substring(firstName.Length, fullName.Length - firstName.Length).Trim(); // all after first name

            await ChangeFirstNameLastNameAsync(clientId, firstName, lastName, changer);
        }

        public async Task ChangeFirstNameLastNameAsync(string clientId, string firstName, string lastName, string changer)
        {
            var fullName = $"{firstName} {lastName}";

            var changes = new KycPersonalDataChanges()
            {
                Changer = changer,
                Items = new Dictionary<string, string>()
                {
                    { nameof(IPersonalData.FirstName), firstName },
                    { nameof(IPersonalData.LastName), lastName },
                    { nameof(IPersonalData.FullName), fullName }
                }
            };

            await _profileService.UpdatePersonalDataAsync(clientId, changes);
        }

        #endregion
    }

    public static class RecordChanger
    {
        public const string Client = "Client";
    }
}
