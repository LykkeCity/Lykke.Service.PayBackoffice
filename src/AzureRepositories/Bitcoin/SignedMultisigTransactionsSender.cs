using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core.BitCoin;

namespace AzureRepositories.Bitcoin
{
    public class SignedMultisigTransactionsSender : ISignedMultisigTransactionsSender
    {
        private readonly IQueueExt _queueExt;

        public SignedMultisigTransactionsSender(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public async Task SendTransaction(SignedTransaction transaction)
        {
            await _queueExt.PutRawMessageAsync(transaction.ToJson());
        }
    }
}
