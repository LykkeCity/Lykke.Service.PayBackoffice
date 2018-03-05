using System;
using System.Threading.Tasks;

namespace Core.BitCoin
{
    public class SignedTransaction
    {
        public Guid? TransactionId { get; set; }
        public string Transaction { get; set; }
    }

    public interface ISignedTransactionsSender
    {
        Task SendTransaction(SignedTransaction transaction);
    }

    public interface ISignedMultisigTransactionsSender
    {
        Task SendTransaction(SignedTransaction transaction);
    }
}
