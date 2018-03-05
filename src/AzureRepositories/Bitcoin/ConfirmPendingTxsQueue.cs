using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core.BitCoin;

namespace AzureRepositories.Bitcoin
{
    public class ConfirmPendingTxsQueue : IConfirmPendingTxsQueue
    {
        private readonly IQueueExt _queueExt;

        public ConfirmPendingTxsQueue(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public Task PutAsync(PendingTxMsg msg)
        {
            return _queueExt.PutRawMessageAsync(msg.ToJson());
        }
    }
}
