using Core.Clients;
using Core.Partner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LkeDomainTest.Mock
{
    public class MockPartnerClientAccountRepository : IPartnerClientAccountRepository
    {
        private Dictionary<Tuple<string, string>, string> _clientAccountsPassword = new Dictionary<Tuple<string, string>, string>()
        {
            { Tuple.Create("clientId", "Lykke"), "Qwerty123456qwerty" }
        };

        private List<IPartnerClientAccount> _clientAccountsRepository = new List<IPartnerClientAccount>()
        {
            new PartnerClientAccount()
            {
                ClientId = "clientId",
                Created = DateTime.UtcNow,
                PublicId = "Lykke",
            },
            new PartnerClientAccount()
            {
                ClientId = "clientId",
                Created = DateTime.UtcNow,
                PublicId = "publicId1",
            }
            ,
            new PartnerClientAccount()
            {
                ClientId = "clientId",
                Created = DateTime.UtcNow,
                PublicId = "publicId2",
            }
        };

        public Task<IPartnerClientAccount> AuthenticateAsync(string clientId, string publicId, string password)
        {
            IPartnerClientAccount client =
                _clientAccountsRepository.FirstOrDefault(x => x.ClientId == clientId && x.PublicId == publicId);

            if (client == null)
            {
                return Task.FromResult<IPartnerClientAccount>(null);
            }

            string oldPassword = _clientAccountsPassword[Tuple.Create(clientId, publicId)];

            return oldPassword == password ? Task.FromResult(client) : Task.FromResult<IPartnerClientAccount>(null);
        }

        public Task ChangePassword(string clientId, string publicId, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IPartnerClientAccount>> GetForClientAsync(string clientId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IPartnerClientAccount>> GetForPartnerAsync(string publicId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsTraderRegisteredForPartnerAsync(string clientId, string publicId)
        {
            IPartnerClientAccount client =
                _clientAccountsRepository.FirstOrDefault(x => x.ClientId == clientId && x.PublicId == publicId);

            return Task.FromResult(clientId != null);
        }

        public Task RegisterAsync(IPartnerClientAccount partner, string password)
        {
            _clientAccountsRepository.Add(partner);
            _clientAccountsPassword[Tuple.Create(partner.ClientId, partner.PublicId)] = password;

            return Task.FromResult(0);
        }
    }
}
