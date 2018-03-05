using System.Threading.Tasks;
using AzureStorage.Queue;
using Common;
using Core.BackgroundWorker;

namespace AzureRepositories.BackgroundWorker
{
    public class BackgroundWorkRequestProducer : IBackgroundWorkRequestProducer
    {
        private readonly IQueueExt _queueExt;

        public BackgroundWorkRequestProducer(IQueueExt queueExt)
        {
            _queueExt = queueExt;
        }

        public Task ProduceRequest<T>(WorkType workType, T context)
        {
            var msg = new BackgroundWorkMessage<T>(workType, context);
            return _queueExt.PutRawMessageAsync(msg.ToJson());
        }
    }
}
