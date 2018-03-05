using BackOffice.Models;
using Common;
using LkeServices.Clients;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Extensions;

namespace BackOffice.Helpers
{
    public static class ClientPartnerRelationHelper
    {
        public static async Task<ClientPartnerRelationIndexViewModel> GetClientPartnerRelationIndexViewModelAsync(
            SrvClientFinder srvClientFinder, IPartnersClient partnersClient, string phrase, string requestUrl)
        {
            var clientAccounts = (await srvClientFinder.FindClientAccountsAsync(phrase)).ToArray();
            var partnerPublicIds = clientAccounts.Select(x => x.PartnerId).Distinct().Except(new[] { string.Empty, null }).ToArray();

            IDictionary<string, Partner> partners =
                (await partnersClient.FindByPublicIdsAsync(partnerPublicIds))?.ToDictionary(x => x.PublicId);

            var searchPersonalData = !phrase.IsValidEmailAndRowKey() ? await srvClientFinder.FindClientsByPhrase(phrase) : null;

            return new ClientPartnerRelationIndexViewModel()
            {
                RequestUrl = requestUrl,
                SearchPersolnalData = searchPersonalData,
                Relations = clientAccounts.Select(x =>
                {
                    string partnerName;
                    if (string.IsNullOrWhiteSpace(x.PartnerId))
                        partnerName = "Lykke";
                    else if (partners != null && partners.ContainsKey(x.PartnerId))
                        partnerName = partners[x.PartnerId].Name;
                    else
                        partnerName = "Partner does not exist";

                    return new ClientPartnerRelationViewModel()
                    {
                        ClientId = x.Id,
                        Email = x.Email,
                        PartnerId = x.PartnerId,
                        PartnerName = partnerName
                    };
                }).ToArray(),
            };
        }
    }
}
