using System;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core.Blockchain.Signature;

namespace AzureRepositories.Blockchain.Signature
{
    public class SignatureCommandProducer : ISignatureCommandProducer
    {
        private readonly IQueueExt _queueExt;

        public SignatureCommandProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public Task SendSignature(Guid requestId, string multisigAddress, string sign, string blockchain)
        {
            var data = new SignedData
            {
                RequestId = requestId,
                MultisigAddress = multisigAddress,
                Signature = sign,
                Blockchain = blockchain
            };
            return _queueExt.PutRawMessageAsync(data.ToJson());
        }

        private class SignedData
        {
            public Guid RequestId { get; set; }
            public string MultisigAddress { get; set; }
            public string Signature { get; set; }
            public string Blockchain { get; set; }
        }


    }
}