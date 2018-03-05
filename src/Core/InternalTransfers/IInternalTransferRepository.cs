using System.Threading.Tasks;

namespace Core.InternalTransfers
{
    public interface IInternalTransferRepository
    {
        Task StoreTransferResult(IInternalTransferRequest request, int? resultCode, string resultMessage, string resultTransactionId);
    }
}
