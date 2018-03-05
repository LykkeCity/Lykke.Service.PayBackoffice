using AzureRepositories.CashOperations;
using Common;
using Common.Log;
using Core.BackOffice;
using Core.BitCoin;
using Core.CashOperations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.PersonalData.Contract;
using MoreLinq;

namespace LkeServices.CashoutRequests
{
    public class CashoutRequestsManager
    {
        private const int RequestsPerPage = 100;

        private readonly ICashOutAttemptRepository _cashOutAttemptRepository;
        private readonly ICashoutRequestLogRepository _cashoutRequestLogRepository;
        private readonly ICashoutTemplateLogRepository _cashoutTemplateLogRepository;
        private readonly ICashoutTemplateRepository _cashoutTemplateRepository;
        private readonly IPersonalDataService _personalDataService;
        private readonly IMenuBadgesRepository _menuBadgesRepository;
        private readonly ICashOperationsRepository _cashOperationsRepository;
        private readonly IBitCoinTransactionsRepository _bitCoinTransactionsRepository;
        private readonly ICashoutPaymentDateRepository _cashoutPaymentDateRepository;
        private readonly ILog _log;
        private readonly ICashoutsProcessedReportRepository _cashoutsProcessedReportRepository;

        public CashoutRequestsManager(
            ICashOutAttemptRepository cashOutAttemptRepository
            , ICashoutRequestLogRepository cashoutRequestLogRepository
            , ICashoutTemplateLogRepository cashoutTemplateLogRepository
            , ICashoutTemplateRepository cashoutTemplateRepository
            , IPersonalDataService personalDataService
            , IMenuBadgesRepository menuBadgesRepository
            , ICashOperationsRepository cashOperationsRepository
            , IBitCoinTransactionsRepository bitCoinTransactionsRepository
            , ICashoutPaymentDateRepository cashoutPaymentDateRepository
            , ILog log
            , ICashoutsProcessedReportRepository cashoutsProcessedReportRepository
            )
        {
            _cashOutAttemptRepository = cashOutAttemptRepository;
            _cashoutRequestLogRepository = cashoutRequestLogRepository;
            _cashoutTemplateLogRepository = cashoutTemplateLogRepository;
            _cashoutTemplateRepository = cashoutTemplateRepository;
            _personalDataService = personalDataService;
            _menuBadgesRepository = menuBadgesRepository;
            _cashOperationsRepository = cashOperationsRepository;
            _bitCoinTransactionsRepository = bitCoinTransactionsRepository;
            _cashoutPaymentDateRepository = cashoutPaymentDateRepository;
            _log = log;
            _cashoutsProcessedReportRepository = cashoutsProcessedReportRepository;

            UpdateMenuBadgesAsync();
        }

        public async Task<ICashOutRequest> SetRequestStatus(CashOutRequestStatus status, string clientId, string requestId, string changer, DateTime? paymentDate = null)
        {
            ICashOutRequest request = null;

            switch (status)
            {
                case CashOutRequestStatus.CanceledByClient:
                    request = await _cashOutAttemptRepository.SetCanceledByClient(clientId, requestId);
                    break;
                case CashOutRequestStatus.CanceledByTimeout:
                    request = await _cashOutAttemptRepository.SetCanceledByTimeout(clientId, requestId);
                    break;
                case CashOutRequestStatus.Confirmed:
                    request = await _cashOutAttemptRepository.SetConfirmed(clientId, requestId);
                    break;
                case CashOutRequestStatus.Declined:
                    request = await _cashOutAttemptRepository.SetDeclined(clientId, requestId);
                    break;
                case CashOutRequestStatus.RequestForDocs:
                    request = await _cashOutAttemptRepository.SetDocsRequested(clientId, requestId);
                    break;
                case CashOutRequestStatus.Pending:
                    request = await _cashOutAttemptRepository.SetPending(clientId, requestId);
                    break;
                case CashOutRequestStatus.Processed:
                    if (!paymentDate.HasValue)
                        throw new ArgumentNullException("paymentDate");
                    request = await _cashOutAttemptRepository.SetProcessed(clientId, requestId);
                    await _cashoutPaymentDateRepository.AddAsync(requestId, paymentDate.Value);
                    MakeProcessedRequestsReportRowAsync(request);
                    break;
            }

            var client = await _personalDataService.GetAsync(request.ClientId);
            await AddLogRecord(changer, request.Id, client.FullName, client.Email, request.Status, request.VolumeSize);
            UpdateMenuBadgesAsync();

            return request;
        }
        
        public async Task<ICashOutRequest> SetHighVolume(string clientId, string requestId, string changer)
        {
            var request = await _cashOutAttemptRepository.SetHighVolume(clientId, requestId);

            var client = await _personalDataService.GetAsync(request.ClientId);
            await AddLogRecord(changer, request.Id, client.FullName, client.Email, request.Status, request.VolumeSize);
            UpdateMenuBadgesAsync();

            return request;
        }

        public async Task<IEnumerable<ICashOutRequest>> GetAllRequestsAsync()
        {
            var requests = (await _cashOutAttemptRepository.GetAllAttempts())
                    .Where(req => (req.State == TransactionStates.SettledOffchain || req.State == TransactionStates.SettledOnchain || req.State == TransactionStates.SettledNoChain))
                    .OrderByDescending(req => req.DateTime);

            return requests;
        }

        public async Task<IEnumerable<ICashOutRequest>> GetProcessedRequestsAsync()
        {
            var requests = (await _cashOutAttemptRepository.GetProcessedAttempts())
                    .Where(req => (req.State == TransactionStates.SettledOffchain || req.State == TransactionStates.SettledOnchain || req.State == TransactionStates.SettledNoChain))
                    .OrderByDescending(req => req.DateTime);

            return requests;
        }

        private Dictionary<int, ICashOutRequest[]> processedRequestsByPage = null;
        public async Task<(IEnumerable<ICashOutRequest> Data, int PageNum, int PagesCount)> GetProcessedRequestsAsync(int pageNum, bool isRefreshData)
        {
            if (pageNum <= 0)
                return (Data: await GetProcessedRequestsAsync(), PageNum: 1, PagesCount: 1);

            if (isRefreshData || processedRequestsByPage == null)
            {
                pageNum = 1;
                processedRequestsByPage = MakeRequestsPagedDictionary(await GetProcessedRequestsAsync());
            }

            return (processedRequestsByPage.Count > 0 && processedRequestsByPage.Count >= pageNum)
                    ? (Data: processedRequestsByPage[pageNum], PageNum: pageNum, PagesCount: processedRequestsByPage.Count)
                    : (Data: new ICashOutRequest[0], PageNum: 1, PagesCount: 1);
        }

        private static Dictionary<int, ICashOutRequest[]> MakeRequestsPagedDictionary(IEnumerable<ICashOutRequest> requests)
        {
            var dict = new Dictionary<int, ICashOutRequest[]>();

            var batches = requests.Batch(RequestsPerPage);

            int currentPage = 0;
            foreach (var batch in batches)
            {
                currentPage++;
                dict.Add(currentPage, batch.ToArray());
            }

            return dict;
        }
        
        private Dictionary<int, ICashOutRequest[]> historyRequestsByPage = null;
        private DateTime historyFromPrevious = DateTime.UtcNow.Date;
        private DateTime historyToPrevious = DateTime.UtcNow.Date.AddMonths(-1);
        public async Task<(IEnumerable<ICashOutRequest> Data, int PageNum, int PagesCount)> GetHistoryRecordsAsync(DateTime? from, DateTime? to, int pageNum, bool isRefreshData)
        {
            from = from ?? historyFromPrevious;
            to = to ?? historyToPrevious;

            if (pageNum <= 0)
                return (Data: await _cashOutAttemptRepository.GetHistoryRecordsAsync(from.Value, to.Value), PageNum: 1, PagesCount: 1);

            if (isRefreshData || historyRequestsByPage == null || historyFromPrevious != from || historyToPrevious != to)
            {
                historyFromPrevious = DateTime.UtcNow.Date;
                historyToPrevious = historyFromPrevious.AddMonths(-1);
                pageNum = 1;
                historyRequestsByPage = MakeRequestsPagedDictionary(await _cashOutAttemptRepository.GetHistoryRecordsAsync(from.Value, to.Value));
            }

            return (historyRequestsByPage.Count > 0 && historyRequestsByPage.Count >= pageNum)
                    ? (Data: historyRequestsByPage[pageNum], PageNum: pageNum, PagesCount: historyRequestsByPage.Count)
                    : (Data: new ICashOutRequest[0], PageNum: 1, PagesCount: 1);
        }

        public async Task<ICashOutRequest> GetRequestAsync(string clientId, string requestId)
        {
            return await _cashOutAttemptRepository.GetAsync(clientId, requestId);
        }

        public async Task<string> InsertRequestAsync<T>(ICashOutRequest transferReq, PaymentSystem paymentSystem, T paymentFields, CashOutRequestTradeSystem tradeSystem)
        {
            var requestId = await _cashOutAttemptRepository.InsertRequestAsync(transferReq, paymentSystem, paymentFields, tradeSystem);

            var client = await _personalDataService.GetAsync(transferReq.ClientId);
            await AddLogRecord("Client", requestId, client.FullName, client.Email, transferReq.Status, transferReq.VolumeSize);
            UpdateMenuBadgesAsync();

            return requestId;
        }

        public async Task<(IEnumerable<ICashoutRequestLogItem> LogRecords, string DeclineReason)> GetLogRecords(string requestId)
        {
            var requests = await _cashOutAttemptRepository.GetRelatedRequestsAsync(requestId);

            var requestIds = new List<string>();
            foreach (var req in requests)
            {
                requestIds.Add(req.Id);
                if (!string.IsNullOrWhiteSpace(req.PreviousId))
                    requestIds.Add(req.PreviousId);
            }
            requestIds = requestIds.Distinct().ToList();

            var declineReason = string.Empty;
            var records = await _cashoutRequestLogRepository.GetRecords(requestIds);
            if (records.Any(rec => rec.Status == CashOutRequestStatus.Declined)) {
                var originalAttempt = records.FirstOrDefault(rec => rec.Status != CashOutRequestStatus.Declined);
                if (originalAttempt != null)
                    declineReason = await GetDeclineReasonAsync(originalAttempt.RequestId);
            }

            return (LogRecords: records.OrderBy(rec => rec.CreationTime), DeclineReason: declineReason);
        }

        private async Task<string> GetDeclineReasonAsync(string requestId)
        {
            var templateLog = await _cashoutTemplateLogRepository.GetDeclineReasonAsync(requestId);
            if (templateLog == null)
                return "No reason";

            var template = await _cashoutTemplateRepository.GetAsync(templateLog.TemplateId);
            
            return $"{template.Text}\r\n{templateLog.Comment}";
        }

        public async Task AddLogRecord(string changer, string requestId, string clientName, string clientEmail, CashOutRequestStatus status, CashOutVolumeSize volumeSize)
        {
            await _cashoutRequestLogRepository.AddRecordAsync(changer, requestId, clientName, clientEmail, status, volumeSize);
        }

        private void UpdateMenuBadgesAsync()
        {
            // No need to wait while that task finished
            Task.Run(async () =>
            {
                var cashoutRequests = (await GetAllRequestsAsync()).ToList();

                var pendingCount = cashoutRequests.Count(x => x.Status == CashOutRequestStatus.Pending);
                var requestedForDocsCount = cashoutRequests.Count(x => x.Status == CashOutRequestStatus.RequestForDocs);
                var highVolumeCount = cashoutRequests.Count(x => (x.Status == CashOutRequestStatus.Pending) && (x.VolumeSize == CashOutVolumeSize.High));
                var lowVolumeCount = cashoutRequests.Count(x => (x.Status == CashOutRequestStatus.Pending) && (x.VolumeSize == CashOutVolumeSize.Low || x.VolumeSize == CashOutVolumeSize.Unknown));

                await _menuBadgesRepository.SaveBadgeAsync(MenuBadges.WithdrawRequest, pendingCount.ToString());
                await _menuBadgesRepository.SaveBadgeAsync(MenuBadges.WithdrawRequestDocs, requestedForDocsCount.ToString());
                await _menuBadgesRepository.SaveBadgeAsync(MenuBadges.WithdrawRequestsHighVolume, highVolumeCount.ToString());
                await _menuBadgesRepository.SaveBadgeAsync(MenuBadges.WithdrawRequestsLowVolume, lowVolumeCount.ToString());
            });
        }

        public async Task<IEnumerable<ICashOutRequest>> GetRelatedRequestsAsync(string requestId)
        {
            return await _cashOutAttemptRepository.GetRelatedRequestsAsync(requestId);
        }

        public async Task<string> FindTransactionIdAsync(ICashOutRequest request)
        {
            string transactionId = null;
            
            var operations = (await _cashOperationsRepository.GetAsync(request.ClientId))
                .Where(op => (op.Amount == request.Amount * -1) && (op.AssetId == request.AssetId))
                .ToList();

            foreach (var operation in operations)
            {
                var transaction = await _bitCoinTransactionsRepository.FindByTransactionIdAsync(operation.TransactionId);
                if (transaction?.ContextData == null)
                    continue;

                try
                {
                    var contextDataJson = JObject.Parse(transaction.ContextData);
                    if (contextDataJson["AddData"].Any() && contextDataJson["AddData"]["SwiftData"].Any())
                    {
                        var requestId = (string)contextDataJson["AddData"]["SwiftData"]["CashOutRequestId"];
                        if ((requestId == request.PreviousId) || (requestId == request.Id))
                        {
                            transactionId = operation.TransactionId;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync(nameof(CashoutRequestsManager), nameof(FindTransactionIdAsync), (new { request.ClientId, request.Id, operation.TransactionId }).ToJson(), ex);
                }
            }
            

            return transactionId;
        }

        public async Task<DateTime?> GetPaymentDateAsync(string requestId) {
            return (await _cashoutPaymentDateRepository.GetAsync(requestId))?.PaymentDate;
        }



        #region Processed Requests report

        private async Task<ICashoutProcessedReportRow> MakeProcessedRequestsReportRowAsync(ICashOutRequest request)
        {
            string transactionId = null;
            DateTime? paymentDate = null;

            if (!string.IsNullOrEmpty(request.PreviousId))
            {
                transactionId = await FindTransactionIdAsync(request);
                paymentDate = await GetPaymentDateAsync(request.PreviousId);
            }


            var reportRow = new CashoutProcessedReportRowEntity()
            {
                RequestId = request.Id,
                ClientId = request.ClientId,
                Amount = request.Amount,
                Currency = request.AssetId,
                SwiftDataJson = request.PaymentFields,
                DateOfRequest = request.DateTime,
                TransactionId = transactionId,
                DateOfPayment = paymentDate
            };

            await _cashoutsProcessedReportRepository.AddAsync(reportRow);

            return reportRow;
        }

        public async Task<List<ICashoutProcessedReportRow>> GenerateProcessedRequestsReportAsync()
        {
            var processedRequests = (await GetAllRequestsAsync())
                .Where(x => x.Status == CashOutRequestStatus.Processed)
                .ToList();

            var existedData = (await _cashoutsProcessedReportRepository.GetDataAsync()).ToDictionary(item => item.RequestId, item => item);
            var result = new List<ICashoutProcessedReportRow>(processedRequests.Count);

            foreach (var request in processedRequests)
            {
                if (existedData.ContainsKey(request.Id))
                {
                    result.Add(existedData[request.Id]);
                    continue;
                }
                
                var reportRow = await MakeProcessedRequestsReportRowAsync(request);
                result.Add(reportRow);
            }

            return result.OrderByDescending(row => row.DateOfRequest).ToList();
        }

        #endregion
    }
}
