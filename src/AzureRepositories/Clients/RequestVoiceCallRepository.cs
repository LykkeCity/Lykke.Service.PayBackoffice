using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class RequestVoiceCallRecordEntity : TableEntity, IRequestVoiceCallRecord
    {
        public static class NewRecords
        {
            public static string GeneratePartitionKey()
            {
                return "VoiceCallReq";
            }

            public static string GenerateRowKey(string clientId)
            {
                return clientId;
            }

            public static RequestVoiceCallRecordEntity Create(string clientId, string phoneNumber)
            {
                return new RequestVoiceCallRecordEntity
                {
                    ClientId = clientId,
                    DateTime = DateTime.UtcNow,
                    PartitionKey = GeneratePartitionKey(),
                    RequestStateVal = (int)RequestState.New,
                    RowKey = GenerateRowKey(clientId),
                    PhoneNumber = phoneNumber
                };
            }
        }

        public static class HistoryRecords
        {
            public static string GeneratePartitionKey()
            {
                return "VoiceCallReqHistory";
            }

            public static RequestVoiceCallRecordEntity Create(IRequestVoiceCallRecord record, RequestState newRequestState, string processedBy)
            {
                return new RequestVoiceCallRecordEntity
                {
                    ClientId = record.ClientId,
                    DateTime = record.DateTime,
                    PartitionKey = GeneratePartitionKey(),
                    RequestStateVal = (int)newRequestState,
                    PhoneNumber = record.PhoneNumber,
                    ProcessedBy = processedBy
                };
            }
        }


        public string ClientId { get; set; }
        public string PhoneNumber { get; set; }
        public string ProcessedBy { get; set; }

        public int RequestStateVal { get; set; }


        public RequestState RequestState
        {
            get { return (RequestState)RequestStateVal; }
            set { RequestStateVal = (int) value; }
        }

        public DateTime DateTime { get; set; }
    }

    public class RequestVoiceCallRepository : IRequestVoiceCallRepository
    {
        private readonly INoSQLTableStorage<RequestVoiceCallRecordEntity> _tableStorage;

        public RequestVoiceCallRepository(INoSQLTableStorage<RequestVoiceCallRecordEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task InsertRequestAsync(string clientId, string phoneNumber)
        {
            var entity = RequestVoiceCallRecordEntity.NewRecords.Create(clientId, phoneNumber);
            return _tableStorage.InsertOrReplaceAsync(entity);
        }

        public async Task HandleRequestProcessedAsync(string clientId, string processedBy)
        {
            var entity = await _tableStorage.DeleteAsync(RequestVoiceCallRecordEntity.NewRecords.GeneratePartitionKey(),
                RequestVoiceCallRecordEntity.NewRecords.GenerateRowKey(clientId));

            await
                _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(RequestVoiceCallRecordEntity.HistoryRecords.Create(entity,
                    RequestState.Processed, processedBy), entity.DateTime);
        }

        public async Task HandleSkipRequestAsync(string clientId, string processedBy)
        {
            var entity = await _tableStorage.DeleteAsync(RequestVoiceCallRecordEntity.NewRecords.GeneratePartitionKey(),
                RequestVoiceCallRecordEntity.NewRecords.GenerateRowKey(clientId));

            await
                _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(RequestVoiceCallRecordEntity.HistoryRecords.Create(entity,
                    RequestState.Skipped, processedBy), entity.DateTime);
        }

        public async Task<IEnumerable<IRequestVoiceCallRecord>> GetNewAsync()
        {
            return await _tableStorage.GetDataAsync(RequestVoiceCallRecordEntity.NewRecords.GeneratePartitionKey());
        }

        public async Task<IEnumerable<IRequestVoiceCallRecord>> 
            GetHistoryRecordsAsync(DateTime @from, DateTime to, Func<IRequestVoiceCallRecord, bool> filter = null)
        {
            to = to.Date.AddDays(1);
            var partitionKey = RequestVoiceCallRecordEntity.HistoryRecords.GeneratePartitionKey();
            return await _tableStorage.WhereAsync(partitionKey, from, to, ToIntervalOption.ExcludeTo, filter);
        }
    }
}
