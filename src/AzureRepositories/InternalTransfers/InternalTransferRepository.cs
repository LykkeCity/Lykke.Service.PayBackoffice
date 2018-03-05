using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.InternalTransfers;

namespace AzureRepositories.InternalTransfers
{
    class InternalTransferRepository : IInternalTransferRepository
    {
        private readonly INoSQLTableStorage<InternalTransferEntity> _tableStorage;

        public InternalTransferRepository(INoSQLTableStorage<InternalTransferEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task StoreTransferResult(IInternalTransferRequest request, int? resultCode, string resultMessage, string resultTransactionId)
        {
            var entity = InternalTransferEntity.Create(request, resultCode, resultMessage, resultTransactionId);

            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity, DateTime.UtcNow);
        }
    }
}
