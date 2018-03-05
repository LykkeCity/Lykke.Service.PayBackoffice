using AzureStorage;
using Common.Log;
using Lykke.Logs;

namespace AzureRepositories.Log
{
    public class RequestsLogPersistenceManager : LogPersistenceManager<RequestsLogRecord>
    {
        public RequestsLogPersistenceManager(
            INoSQLTableStorage<RequestsLogRecord> tableStorage, 
            ILog log = null) : 
            
            base(
                nameof(RequestsLogPersistenceManager), 
                tableStorage, 
                new RequestsLogEntityRowGenerator(), 
                log)
        {
        }
    }
}
