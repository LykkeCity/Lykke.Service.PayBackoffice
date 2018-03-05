using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IClientPartner
    {
        string Id { get; }
        string ClientId { get; }
        string PartnerPublicId { get; }
        DateTime CreatedAt { get; }
    }

    public class ClientPartner : IClientPartner
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string PartnerPublicId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public interface IClientPartnerRepository
    {
        Task<IEnumerable<IClientPartner>> GetClientPartnerAsync(IEnumerable<string> clientIds);
        Task<IEnumerable<IClientPartner>> GetClientPartnerByPartnerIdAsync(string partnerPublicId);
        Task CreateClientPartnerAsync(IClientPartner clientPartner);
        Task DeleteClientPartnerAsync(IClientPartner clientPartner);
    }
}