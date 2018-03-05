using AutoMapper;
using AzureStorage;
using Core.CashOperations;
using Core.CashOperations.PaymentSystems;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureRepositories.CashOperations
{
    public class CashoutProcessedReportRowEntity : TableEntity, ICashoutProcessedReportRow
    {
        public string ClientId { get; set; }
        public string RequestId { get; set; }
        public string Currency { get; set; }
        public DateTime DateOfRequest { get; set; }
        public string TransactionId { get; set; }
        public double Amount { get; set; }
        public DateTime? DateOfPayment { get; set; }
        public Swift SwiftData { get; set; }
        public string SwiftDataJson
        {
            get { return JsonConvert.SerializeObject(SwiftData); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SwiftData = null;

                SwiftData = JsonConvert.DeserializeObject<Swift>(value);
            }
        }

        internal static string GetPartitionKey() => "CashoutsProcessed";
        internal static string GetRowKey(string requestId) => requestId;
    }

    public class CashoutsProcessedReportRepository : ICashoutsProcessedReportRepository
    {
        private readonly INoSQLTableStorage<CashoutProcessedReportRowEntity> _storage;

        public CashoutsProcessedReportRepository(INoSQLTableStorage<CashoutProcessedReportRowEntity> storage)
        {
            _storage = storage;
        }

        public async Task<List<ICashoutProcessedReportRow>> GetDataAsync()
        {
            var data = await _storage.GetDataAsync();
            var result = data.Cast<ICashoutProcessedReportRow>().ToList();
            return result;
        }

        public async Task AddAsync(ICashoutProcessedReportRow src)
        {
            var row = Mapper.Map<CashoutProcessedReportRowEntity>(src);

            row.PartitionKey = CashoutProcessedReportRowEntity.GetPartitionKey();
            row.RowKey = CashoutProcessedReportRowEntity.GetRowKey(row.RequestId);

            await _storage.InsertOrReplaceAsync(row);
        }
    }
}
