using Common.PasswordTools;
using Core.Partner;
using System.Threading.Tasks;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;
using System.Collections.Generic;
using System.Linq;

namespace LkeDomain.Credentials
{
    public class ClientAccountLogic
    {
        private readonly IClientAccountClient _clientAccountService;
        private readonly IPartnerAccountPolicyRepository _partnerAccountPolicyRepository;

        public ClientAccountLogic(IClientAccountClient clientAccountService,
            IPartnerAccountPolicyRepository partnerAccountPolicyRepository
            )
        {
            _clientAccountService = clientAccountService;
            _partnerAccountPolicyRepository = partnerAccountPolicyRepository;
        }

        public async Task<ClientAccountInformationModel> AuthenticateUser(string email, string password, string partnerPublicId = null)
        {
            //Here we substitute publicId according to partner client account settings
            string publicId = await GetPartnerIdAccordingToSettings(partnerPublicId);

            var client = await _clientAccountService.AuthenticateAsync(email, password, publicId) ??
                         await _clientAccountService.AuthenticateAsync(email,
                             PasswordKeepingUtils.GetClientHashedPwd(password), publicId);

            return client;
        }

        public async Task<ClientAccountInformationModel> IsTraderWithEmailExistsForPartnerAsync(string email, string partnerId = null)
        {
            string partnerIdAccordingToPolicy = await GetPartnerIdAccordingToSettings(partnerId);
            return await _clientAccountService.GetClientByEmailAndPartnerIdAsync(email, partnerIdAccordingToPolicy);
        }

        public async Task<ClientAccountInformationModel> IsTraderWithPhoneExistsForPartnerAsync(string phoneNumber, string partnerId = null)
        {
            string partnerIdAccordingToPolicy = await GetPartnerIdAccordingToSettings(partnerId);
            return await _clientAccountService.GetClientByPhoneAndPartnerIdAsync(phoneNumber, partnerIdAccordingToPolicy);
        }

        public async Task InsertIndexedByPhoneAsync(string clientId, string phoneNumber, string previousPhoneNumber)
        {
            await _clientAccountService.InsertIndexedByPhoneAsync(clientId, phoneNumber, previousPhoneNumber);
        }

        public async Task<bool> IsClientAccountActive(string clientId)
        {
            return !(await _clientAccountService.IsClientBannedAsync(clientId));
        }


        #region PrivateMethods

        /// <summary>
        /// Method returns true if we use different from LykkeWallet credentials else returns false
        /// </summary>
        public async Task<bool> UsePartnerCredentials(string partnerPublicId)
        {
            bool usePartnerCredentials = false;
            if (!string.IsNullOrEmpty(partnerPublicId))
            {
                IPartnerAccountPolicy policy = await _partnerAccountPolicyRepository.GetAsync(partnerPublicId);
                usePartnerCredentials = policy?.UseDifferentCredentials ?? false;
            }

            return usePartnerCredentials;
        }

        private async Task<string> GetPartnerIdAccordingToSettings(string partnerPublicId)
        {
            bool usePartnerCredentials = await UsePartnerCredentials(partnerPublicId);
            //Depends on partner settings
            string publicId = !usePartnerCredentials ? null : partnerPublicId;

            return publicId;
        }

        #endregion PrivateMethods
    }
}
