using Core.Clients;
using Core.Partner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LkeDomainTest.Mock
{
    public class MockPartnerAccountPolicyRepository : IPartnerAccountPolicyRepository
    {
        private List<IPartnerAccountPolicy> _clientAccountsRepository = new List<IPartnerAccountPolicy>()
        {
            new PartnerAccountPolicy
            {
               PublicId = "publicId1",
               UseDifferentCredentials = true
            },
            new PartnerAccountPolicy
            {
               PublicId = "publicId2",
               UseDifferentCredentials = false
            }
        };

        public Task CreateAsync(IPartnerAccountPolicy partner)
        {
            _clientAccountsRepository.Add(partner);

            return Task.FromResult(0);
        }

        public Task CreateOrUpdateAsync(IPartnerAccountPolicy newPolicy)
        {
            IPartnerAccountPolicy policy = _clientAccountsRepository.FirstOrDefault(x => x.PublicId == newPolicy.PublicId);
            if (policy == null)
            {
                _clientAccountsRepository.Add(newPolicy);
            }

            policy.UseDifferentCredentials = newPolicy.UseDifferentCredentials;

            return Task.FromResult(0);
        }

        public Task<IPartnerAccountPolicy> GetAsync(string publicId)
        {
            IPartnerAccountPolicy policy = _clientAccountsRepository.FirstOrDefault(x => x.PublicId == publicId);

            return Task.FromResult(policy);
        }

        public Task<IEnumerable<IPartnerAccountPolicy>> GetPoliciesAsync()
        {
            return Task.FromResult((IEnumerable<IPartnerAccountPolicy>)_clientAccountsRepository);
        }

        public Task RemoveAsync(string publicId)
        {
            IPartnerAccountPolicy policy = _clientAccountsRepository.FirstOrDefault(x => x.PublicId == publicId);
            _clientAccountsRepository.Remove(policy);

            return Task.FromResult(0);
        }

        public Task UpdateAsync(IPartnerAccountPolicy partner)
        {
            throw new NotImplementedException();
        }
    }
}
