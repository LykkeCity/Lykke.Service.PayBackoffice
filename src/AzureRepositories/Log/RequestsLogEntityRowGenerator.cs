using Core.EventLogs;
using Lykke.Logs;

namespace AzureRepositories.Log
{
    public class RequestsLogEntityRowGenerator : ILogEntityRowKeyGenerator<IRequestsLogRecord>
    {
        public string Generate(IRequestsLogRecord entity, int retryNum, int batchItemNum)
        {
            return RequestsLogRecord.GenerateRowKey(entity.DateTime, retryNum, batchItemNum);
        }
    }
}