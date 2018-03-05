using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;
using AzureStorage.Tables.Templates.Index;

namespace AzureRepositories.Clients
{
    public class ClientPartnerEntity : TableEntity, IClientPartner
    {
        public string Id => RowKey;

        public string ClientId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string PartnerPublicId { get; set; }

        public static string GeneratePartitionKey(string additionalKey)
        {
            return $"ClientPartner_{additionalKey}";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public static ClientPartnerEntity CreateNew(IClientPartner clientPartner, string additionalKey)
        {
            var result = new ClientPartnerEntity
            {
                PartitionKey = GeneratePartitionKey(additionalKey),
                RowKey = !string.IsNullOrEmpty(clientPartner.Id) ? Guid.NewGuid().ToString() : clientPartner.Id,
                ClientId = clientPartner.ClientId,
                PartnerPublicId = clientPartner.PartnerPublicId,
                CreatedAt = clientPartner.CreatedAt,
            };

            return result;
        }

        public static string GeneratePartnerPublicIdKey(string publicPartnerId)
        {
            return $"PartnerPublicId_{publicPartnerId}";
        }

        public static string GenerateClientIdKey(string clientIdKey)
        {
            return $"ClientId_{clientIdKey}";
        }
    }


    public class ClientPartnerRepository : IClientPartnerRepository
    {
        private readonly INoSQLTableStorage<ClientPartnerEntity> _clientPartnerTablestorage;
        public ClientPartnerRepository(INoSQLTableStorage<ClientPartnerEntity> clientPartnerTablestorage)
        {
            _clientPartnerTablestorage = clientPartnerTablestorage;
        }

        public async Task CreateClientPartnerAsync(IClientPartner clientPartner)
        {
            ClientPartnerEntity newEntityPartnerPartition =
                ClientPartnerEntity.CreateNew(clientPartner, clientPartner.PartnerPublicId);
            ClientPartnerEntity newEntityClientPartition =
                ClientPartnerEntity.CreateNew(clientPartner, clientPartner.ClientId);

            await _clientPartnerTablestorage.InsertAsync(newEntityPartnerPartition);
            await _clientPartnerTablestorage.InsertAsync(newEntityClientPartition);
        }

        public async Task DeleteClientPartnerAsync(IClientPartner clientPartner)
        {
            ClientPartnerEntity entityPartnerPartition =
                ClientPartnerEntity.CreateNew(clientPartner, clientPartner.PartnerPublicId);
            ClientPartnerEntity entityClientPartition =
                ClientPartnerEntity.CreateNew(clientPartner, clientPartner.ClientId);

            await _clientPartnerTablestorage.DeleteAsync(entityPartnerPartition);
            await _clientPartnerTablestorage.DeleteAsync(entityClientPartition);
        }

        public async Task<IEnumerable<IClientPartner>> GetClientPartnerAsync(IEnumerable<string> clientIds)
        {
            var partitionKeys = clientIds.Select(x =>
            {
                string key = ClientPartnerEntity.GenerateClientIdKey(x);
                return ClientPartnerEntity.GeneratePartitionKey(key);
            });

            return await _clientPartnerTablestorage.GetDataAsync(partitionKeys);
        }

        public async Task<IEnumerable<IClientPartner>> GetClientPartnerByPartnerIdAsync(string publicPartnerId)
        {
            string partnerKey = ClientPartnerEntity.GeneratePartnerPublicIdKey(publicPartnerId);
            var partitionKey = ClientPartnerEntity.GeneratePartitionKey(partnerKey);

            return await _clientPartnerTablestorage.GetDataAsync(partitionKey);
        }
    }
}
