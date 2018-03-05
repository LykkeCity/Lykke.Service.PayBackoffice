using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AzureStorage;
using Common;
using Common.Log;
using Core.EventLogs;
using Lykke.Logs;

namespace AzureRepositories.Log
{
    public class RequestsLogRepository :
        TimerPeriod,
        IRequestsLogRepository
    {
        private readonly INoSQLTableStorage<RequestsLogRecord> _tableStorage;
        private readonly ILogPersistenceManager<RequestsLogRecord> _persistenceManager;
        private readonly TimeSpan _maxBatchLifetime;
        private readonly int _batchSizeThreshold;
        private readonly bool _ownPersistenceManager;

        private List<RequestsLogRecord> _currentBatch;
        private volatile int _currentBatchSize;
        private int _maxBatchSize;
        private DateTime _currentBatchDeathtime;


        /// <param name="tableStorage">Table storage from which records will be gotten</param>
        /// <param name="persistenceManager">Persistence manager</param>
        /// <param name="log">log, which will be used to log logging infrastructure's issues</param>
        /// <param name="maxBatchLifetime">Log entries batch's lifetime, when exceeded, batch will be saved, and new batch will be started. Default is 5 seconds</param>
        /// <param name="batchSizeThreshold">Log messages batch's size threshold, when exceeded, batch will be saved, and new batch will be started. Default is 100 entries</param>
        /// <param name="ownPersistenceManager">Is log instance owns persistence manager: should it manages Start/Stop</param>
        public RequestsLogRepository(
            INoSQLTableStorage<RequestsLogRecord> tableStorage,
            ILogPersistenceManager<RequestsLogRecord> persistenceManager,
            ILog log,
            TimeSpan? maxBatchLifetime = null,
            int batchSizeThreshold = 100,
            bool ownPersistenceManager = true) :
            base(nameof(RequestsLogRepository), periodMs: 20, log: log)
        {
            _tableStorage = tableStorage;
            _persistenceManager = persistenceManager;
            _batchSizeThreshold = batchSizeThreshold;
            _ownPersistenceManager = ownPersistenceManager;
            _maxBatchLifetime = maxBatchLifetime ?? TimeSpan.FromSeconds(5);
        }

        public override void Start()
        {
            StartNewBatch();

            if (_ownPersistenceManager)
            {
                (_persistenceManager as IStartable)?.Start();
            }

            base.Start();
        }

        public override void Stop()
        {
            lock (_currentBatch)
            {
                _persistenceManager.Persist(_currentBatch);
                _currentBatch = null;
            }

            base.Stop();

            if (_ownPersistenceManager)
            {
                (_persistenceManager as IStopable)?.Stop();
            }
        }

        public void Write(string clientId, string url, string request, string response, string userAgent)
        {
            if (_currentBatch == null)
            {
                return;
            }

            var newEntity = RequestsLogRecord.Create(clientId, url, request, response, userAgent);

            lock (_currentBatch)
            {
                _currentBatch.Add(newEntity);
                ++_currentBatchSize;
            }
        }

        public async Task<IEnumerable<IRequestsLogRecord>> GetRecords(string clientId, DateTime from, DateTime to)
        {
            var partitionKey = RequestsLogRecord.GeneratePartitionKey(clientId);

            return (await _tableStorage.WhereAsync(partitionKey, from, to.Date.AddDays(1), ToIntervalOption.ExcludeTo)).OrderByDescending(x => x.DateTime);
        }

        public override Task Execute()
        {
            if (_currentBatch == null)
            {
                return Task.CompletedTask;
            }

            IReadOnlyCollection<RequestsLogRecord> batchToSave = null;

            var now = DateTime.UtcNow;

            if (_currentBatchSize >= _batchSizeThreshold || _currentBatchSize > 0 && now >= _currentBatchDeathtime)
            {
                lock (_currentBatch)
                {
                    if (_currentBatchSize >= _batchSizeThreshold || _currentBatchSize > 0 && now >= _currentBatchDeathtime)
                    {
                        batchToSave = _currentBatch;
                        StartNewBatch();
                    }
                }
            }

            if (batchToSave != null)
            {
                _persistenceManager.Persist(batchToSave);
            }

            return Task.CompletedTask;
        }

        private void StartNewBatch()
        {
            _maxBatchSize = Math.Max(_maxBatchSize, _currentBatchSize);
            _currentBatch = new List<RequestsLogRecord>(_maxBatchSize);
            _currentBatchSize = 0;
            _currentBatchDeathtime = DateTime.UtcNow + _maxBatchLifetime;
        }
    }
}
