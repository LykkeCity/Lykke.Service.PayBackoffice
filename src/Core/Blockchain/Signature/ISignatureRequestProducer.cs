using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Blockchain.Signature
{
    public interface ISignatureRequest
    {
        string ClientId { get; set; }
        Guid RequestId { get; set; }
        string MultisigAddress { get; set; }
        string Blockchain { get; set; }
        string Hash { get; set; }
        bool IsSigned { get; set; }
        string Sign { get; set; }
    }

    public class SignatureRequest : ISignatureRequest
    {
        public string ClientId { get; set; }
        public Guid RequestId { get; set; }
        public string MultisigAddress { get; set; }
        public string Blockchain { get; set; }
        public string Hash { get; set; }
        public bool IsSigned { get; set; }
        public string Sign { get; set; }
    }



    public interface ISignatureRequestRepository
    {
        Task Insert(ISignatureRequest signatureRequest);
        Task<IEnumerable<ISignatureRequest>> GetRequestsOfClient(string clientId);
        Task<ISignatureRequest> MarkAsSigned(string clientId, Guid requestId, string multisigAddress, string sign);
        Task<ISignatureRequest> GetSignatureRequest(string clientId, Guid requestId, string multisigAddress);

    }
}