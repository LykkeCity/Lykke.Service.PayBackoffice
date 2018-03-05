﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common;
using Core.Ethereum;

namespace AzureRepositories.Ethereum
{
    public class EthereumTransactionReqEntity : BaseEntity, IEthereumTransactionRequest
    {
        public static class ById
        {
            public static string GeneratePartition()
            {
                return "ETR";
            }

            public static string GenerateRowKey(Guid id)
            {
                return id.ToString();
            }

            public static EthereumTransactionReqEntity Create(IEthereumTransactionRequest request)
            {
                var entity = Mapper.Map<EthereumTransactionReqEntity>(request);
                entity.PartitionKey = GeneratePartition();
                entity.RowKey = GenerateRowKey(request.Id);

                return entity;
            }
        }

        public static class ByOrderId
        {
            public static string GeneratePartition()
            {
                return "OrderId";
            }

            public static string GenerateRowKey(string orderId)
            {
                return orderId;
            }

            public static EthereumTransactionReqEntity Create(IEthereumTransactionRequest request)
            {
                var entity = Mapper.Map<EthereumTransactionReqEntity>(request);
                entity.PartitionKey = GeneratePartition();
                entity.RowKey = GenerateRowKey(request.OrderId);

                return entity;
            }
        }

        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public string Hash { get; set; }
        public string AssetId { get; set; }
        public decimal Volume { get; set; }
        public string AddressTo { get; set; }
        public string OrderId { get; set; }
        public bool Used { get; set; }

        public string SignedTransferVal { get; set; }
        public Transaction SignedTransfer
        {
            get { return SignedTransferVal?.DeserializeJson<Transaction>(); }
            set { SignedTransferVal = value.ToJson(); }
        }

        public OperationType OperationType { get; set; }

        public string OperationIdsVal { get; set; }
        public string[] OperationIds
        {
            get { return OperationIdsVal?.DeserializeJson<string[]>(); }
            set { OperationIdsVal = value.ToJson(); }
        }

    }

    public class EthereumTransactionRequestRepository : IEthereumTransactionRequestRepository
    {
        private readonly INoSQLTableStorage<EthereumTransactionReqEntity> _tableStorage;

        public EthereumTransactionRequestRepository(INoSQLTableStorage<EthereumTransactionReqEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEthereumTransactionRequest> GetAsync(Guid id)
        {
            var entity = await _tableStorage.GetDataAsync(EthereumTransactionReqEntity.ById.GeneratePartition(),
                EthereumTransactionReqEntity.ById.GenerateRowKey(id));
            return entity;
        }

        public async Task<IEthereumTransactionRequest> GetAndMarkAsync(Guid id)
        {
            return await _tableStorage.MergeAsync(EthereumTransactionReqEntity.ById.GeneratePartition(),
                EthereumTransactionReqEntity.ById.GenerateRowKey(id), entity =>
                {
                    // we've already used this request
                    if (entity.Used)
                        return null;

                    entity.Used = true;
                    return entity;
                }
            );
        }

        public async Task InsertAsync(IEthereumTransactionRequest request)
        {
            var entityById = EthereumTransactionReqEntity.ById.Create(request);
            await _tableStorage.InsertAsync(entityById);

            if (!string.IsNullOrEmpty(request.OrderId))
            {
                var entitByOrder = EthereumTransactionReqEntity.ByOrderId.Create(request);
                await _tableStorage.InsertAsync(entitByOrder);
            }
        }

        public async Task<IEthereumTransactionRequest> GetByOrderAsync(string orderId)
        {
            var entity = await _tableStorage.GetDataAsync(EthereumTransactionReqEntity.ByOrderId.GeneratePartition(),
                EthereumTransactionReqEntity.ByOrderId.GenerateRowKey(orderId));
            return entity;
        }

        public async Task UpdateAsync(IEthereumTransactionRequest request)
        {
            var byId = EthereumTransactionReqEntity.ById.Create(request);
            await _tableStorage.InsertOrReplaceAsync(byId);

            if (!string.IsNullOrEmpty(request.OrderId))
            {
                var byOrder = EthereumTransactionReqEntity.ByOrderId.Create(request);
                await _tableStorage.InsertOrReplaceAsync(byOrder);
            }
        }
    }
}
