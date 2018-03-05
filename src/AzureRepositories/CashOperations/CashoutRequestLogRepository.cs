using AzureStorage;
using Core.CashOperations;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureRepositories.CashOperations
{
    public class CashoutRequestLogRecord : TableEntity, ICashoutRequestLogItem
    {
        public static string GeneratePartitionKey(string requestId) => $"CashoutRequestStatus_{requestId}";
        public static CashoutRequestLogRecord Create(string changer, string requestId, string clientName, string clientEmail, CashOutRequestStatus status, CashOutVolumeSize volumeSize)
        {
            var logRecord = new CashoutRequestLogRecord();

            logRecord.CreationTime = DateTime.UtcNow;
            logRecord.PartitionKey = GeneratePartitionKey(requestId);
            logRecord.RequestId = requestId;
            logRecord.Changer = changer;
            logRecord.ClientName = clientName;
            logRecord.ClientEmail = clientEmail;
            logRecord.Status = status;
            logRecord.VolumeSize = volumeSize;

            return logRecord;
        }


        public DateTime CreationTime { get; set; }
        public string RequestId { get; set; }

        public string Changer { get; set; }

        public string ClientName { get; set; }

        public string ClientEmail { get; set; }

        public CashOutRequestStatus Status { get; set; }

        public string StatusText
        {
            get { return Status.ToString(); }
            set
            {
                CashOutRequestStatus status;
                Enum.TryParse(value, out status);
                Status = status;
            }
        }

        public CashOutVolumeSize VolumeSize { get; set; }

        public string VolumeSizeText
        {
            get { return VolumeSize.ToString(); }
            set
            {
                CashOutVolumeSize volumeSize;
                Enum.TryParse(value, out volumeSize);
                VolumeSize = volumeSize;
            }
        }
    }

    public class CashoutRequestLogRepository : ICashoutRequestLogRepository
    {
        readonly INoSQLTableStorage<CashoutRequestLogRecord> _tableStorage;

        public CashoutRequestLogRepository(INoSQLTableStorage<CashoutRequestLogRecord> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task AddRecordAsync(string changer, string requestId, string clientName, string clientEmail, CashOutRequestStatus status, CashOutVolumeSize volumeSize)
        {
            var record = CashoutRequestLogRecord.Create(changer, requestId, clientName, clientEmail, status, volumeSize);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(record, record.CreationTime);
        }

        public async Task<IEnumerable<ICashoutRequestLogItem>> GetRecords(string requestId)
        {
            var pk = CashoutRequestLogRecord.GeneratePartitionKey(requestId);
            var records = await _tableStorage.GetDataAsync(pk);
            return records;
        }

        public async Task<IEnumerable<ICashoutRequestLogItem>> GetRecords(IEnumerable<string> requestIds)
        {
            var pks = requestIds.Select(requestId => CashoutRequestLogRecord.GeneratePartitionKey(requestId));
            var records = await _tableStorage.GetDataAsync(pks);
            return records;
        }
    }
}
