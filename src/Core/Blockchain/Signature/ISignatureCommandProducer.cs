using System;
using System.Threading.Tasks;

namespace Core.Blockchain.Signature
{
    public interface ISignatureCommandProducer
    {
        Task SendSignature(Guid requestId, string multisigAddress, string sign, string blockchain);
    }
}
