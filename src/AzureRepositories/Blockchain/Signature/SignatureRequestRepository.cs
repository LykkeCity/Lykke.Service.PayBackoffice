using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Blockchain.Signature;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Blockchain.Signature
{
    public class SignatureRequestEntity : TableEntity, ISignatureRequest
    {

        public static class ByClientId
        {
            public static string GeneratePartitionKey(string clientId)
            {
                return "ClientId_" + clientId;
            }

            public static string GenerateRowKey(Guid requestId, string multisig)
            {
                return requestId + "_" + multisig;
            }

            public static SignatureRequestEntity Create(ISignatureRequest request)
            {
                return new SignatureRequestEntity
                {
                    PartitionKey = GeneratePartitionKey(request.ClientId),
                    RowKey = GenerateRowKey(request.RequestId, request.MultisigAddress),
                    RequestId = request.RequestId,
                    ClientId = request.ClientId,
                    MultisigAddress = request.MultisigAddress,
                    Blockchain = request.Blockchain,
                    Hash = request.Hash,
                    IsSigned = request.IsSigned,
                    Sign = request.Sign
                };
            }
        }


        public string ClientId { get; set; }
        public Guid RequestId { get; set; }
        public string MultisigAddress { get; set; }
        public string Blockchain { get; set; }
        public string Hash { get; set; }
        public bool IsSigned { get; set; }
        public string Sign { get; set; }
    }



    public class SignatureRequestRepository : ISignatureRequestRepository
    {
        private readonly INoSQLTableStorage<SignatureRequestEntity> _table;

        public SignatureRequestRepository(INoSQLTableStorage<SignatureRequestEntity> table)
        {
            _table = table;
        }

        public async Task Insert(ISignatureRequest signatureRequest)
        {
            await _table.InsertAsync(SignatureRequestEntity.ByClientId.Create(signatureRequest));
        }

        public async Task<IEnumerable<ISignatureRequest>> GetRequestsOfClient(string clientId)
        {
            var partitionKey = SignatureRequestEntity.ByClientId.GeneratePartitionKey(clientId);
            var records = await _table.GetDataAsync(partitionKey, entity => !entity.IsSigned);
            return records;
        }

        public async Task<ISignatureRequest> MarkAsSigned(string clientId, Guid requestId, string multisigAddress, string sign)
        {
            return await _table.ReplaceAsync(SignatureRequestEntity.ByClientId.GeneratePartitionKey(clientId),
                SignatureRequestEntity.ByClientId.GenerateRowKey(requestId, multisigAddress), entity =>
                {
                    entity.IsSigned = true;
                    entity.Sign = sign;
                    return entity;
                });
        }

        public async Task<ISignatureRequest> GetSignatureRequest(string clientId, Guid requestId, string multisigAddress)
        {
            return await _table.GetDataAsync(SignatureRequestEntity.ByClientId.GeneratePartitionKey(clientId),
                SignatureRequestEntity.ByClientId.GenerateRowKey(requestId, multisigAddress));
        }
    }
}