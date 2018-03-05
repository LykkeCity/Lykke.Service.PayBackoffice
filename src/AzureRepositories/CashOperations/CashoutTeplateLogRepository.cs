using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.CashOperations;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.CashOperations
{
    public class CashoutTemplateLogRecord : TableEntity, ICashoutLogRecord
    {
        public static string GeneratePartitionKey(CashoutTemplateType type) => $"CashoutTemplateLog_{type}";

        public static CashoutTemplateLogRecord Create(ICashoutLogRecord src, CashoutTemplateType type)
        {
            var partitionKey = GeneratePartitionKey(type);

            return new CashoutTemplateLogRecord()
            {
                RequestId = src.RequestId,
                CreationTime = src.CreationTime,
                TemplateId = src.TemplateId,
                ClientId = src.ClientId,
                Comment = src.Comment,
                Changer = src.Changer,
                TypeText = type.ToString(),

                PartitionKey = partitionKey,
            };
        }


        public DateTime CreationTime { get; set; }

        public string ClientId { get; set; }
        public string RequestId { get; set; }
        public string TemplateId { get; set; }
        public string Comment { get; set; }
        public string Changer { get; set; }
        public CashoutTemplateType Type { get; set; }

        public string TypeText
        {
            get { return Type.ToString(); }
            set { Type = value.ToCashoutTemplateType(); }
        }

        public CashoutTemplateLogRecord()
        {
            CreationTime = DateTime.UtcNow;
        }
    }

    public class CashoutTemplateLogRepository : ICashoutTemplateLogRepository
    {
        private readonly INoSQLTableStorage<CashoutTemplateLogRecord> _tableStorage;

        public CashoutTemplateLogRepository(INoSQLTableStorage<CashoutTemplateLogRecord> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task AddRecordAsync(ICashoutLogRecord record, CashoutTemplateType type)
        {
            var entity = CashoutTemplateLogRecord.Create(record, type);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity, DateTime.UtcNow);
        }

        public async Task<IEnumerable<ICashoutLogRecord>> GetAllRecordsAsync()
        {
            return await _tableStorage.GetDataAsync();
        }
        
        public async Task<ICashoutLogRecord> GetDeclineReasonAsync(string requestId)
        {
            var partitionKeyCond = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CashoutTemplateLogRecord.GeneratePartitionKey(CashoutTemplateType.Decline));
            var requestIdCond = TableQuery.GenerateFilterCondition(nameof(CashoutTemplateLogRecord.RequestId), QueryComparisons.Equal, requestId);
            var query = new TableQuery<CashoutTemplateLogRecord>
            {
                FilterString = TableQuery.CombineFilters(partitionKeyCond, TableOperators.And, requestIdCond)
            };

            var logRecords = await _tableStorage.WhereAsync(query);

            return logRecords.FirstOrDefault();
        }

    }
}
