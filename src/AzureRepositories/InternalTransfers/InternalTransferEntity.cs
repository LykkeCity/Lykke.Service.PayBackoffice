using Core.InternalTransfers;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.InternalTransfers
{
    public class InternalTransferEntity : TableEntity
    {
        public string AccountFrom { get; set; }
        public string AssetId { get; set; }
        public string AccountTo { get; set; }
        public double Amount { get; set; }
        public string Comment { get; set; }

        public int? ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public string ResultTransactionId { get; set; }

        public static InternalTransferEntity Create(IInternalTransferRequest request, int? resultCode, string resultMessage, string resultTransactionId)
        {
            return new InternalTransferEntity()
            {
                PartitionKey = request.AccountFrom,

                AccountFrom = request.AccountFrom,
                AccountTo = request.AccountTo,
                Amount = request.Amount,
                AssetId = request.AssetId,
                Comment = request.Comment,
                ResultCode = resultCode,
                ResultMessage = resultMessage,
                ResultTransactionId = resultTransactionId
            };
        }
    }
}
