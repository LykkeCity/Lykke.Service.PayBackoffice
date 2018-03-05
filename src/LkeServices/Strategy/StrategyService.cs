using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.MmSettings;
using Core.Strategy;

namespace LkeServices.Strategy
{
    public class StrategyService : IStrategyService
    {
        private readonly IMmApiClient _mmApiClient;
        private readonly IMmSettingsAuditLogRepository _mmSettingsAuditLogRepository;

        public StrategyService(IMmApiClient mmApiClient, IMmSettingsAuditLogRepository mmSettingsAuditLogRepository)
        {
            _mmApiClient = mmApiClient;
            _mmSettingsAuditLogRepository = mmSettingsAuditLogRepository;
        }

        public async Task<IEnumerable<IStrategy>> GetStrategies()
        {
            var strategies = await _mmApiClient.GetStrategies();

            return strategies.Select(item => new Core.Strategy.Strategy
                {
                    AssetPairId = item.AssetPairId,
                    Type = item.Type
                })
                .Cast<IStrategy>()
                .ToList();
        }

        public async Task<IAveragePriceMovement> GetAveragePriceMovement(string assetPairId)
        {
            var apiRecord = await _mmApiClient.GetAveragePriceMovement(assetPairId);

            var averagePriceMovement = new AveragePriceMovement
            {
                AssetPairId = assetPairId,
                AskIncrementSpread = apiRecord.AskIncrementSpread,
                BaseSpread = apiRecord.BaseSpread,
                BidIncrementSpread = apiRecord.BidIncrementSpread,
                RelaxationTime = apiRecord.RelaxationTime
            };

            return averagePriceMovement;
        }

        public async Task<IMarkUp> GetMarkUp(string assetPairId)
        {
            var apiRecord = await _mmApiClient.GetMarkUp(assetPairId);

            var markUp = new MarkUp
            {
                AssetPairId = assetPairId,
                Type = apiRecord.Type,
                AskMarkUp = apiRecord.AskMarkUp,
                BidMarkUp = apiRecord.BidMarkUp,
            };

            return markUp;
        }

        public async Task EditAveragePriceMovement(string userId, string assetPairId, IEditAveragePriceMovement averagePriceMovement)
        {
            var apiRecord = await _mmApiClient.GetAveragePriceMovement(assetPairId);
            var beforeJsonToLog = apiRecord.ToJson();

            apiRecord.AskIncrementSpread = decimal.Parse(averagePriceMovement.AskIncrementSpread, CultureInfo.InvariantCulture);
            apiRecord.BaseSpread = decimal.Parse(averagePriceMovement.BaseSpread, CultureInfo.InvariantCulture);
            apiRecord.BidIncrementSpread = decimal.Parse(averagePriceMovement.BidIncrementSpread, CultureInfo.InvariantCulture);
            apiRecord.RelaxationTime = decimal.Parse(averagePriceMovement.RelaxationTime, CultureInfo.InvariantCulture);
            
            await _mmApiClient.EditAveragePriceMovement(assetPairId, apiRecord);

            await _mmSettingsAuditLogRepository.InsertRecord(
                new MmSettingsAuditLogData
                {
                    AssetPairId = assetPairId,
                    UserId = userId,
                    CreatedTime = DateTime.UtcNow,
                    RecordType = MmSettingsAuditRecordType.EditAveragePriceMovement,
                    BeforeJson = beforeJsonToLog,
                    AfterJson = apiRecord.ToJson()
                });
        }

        public async Task EditMarkUp(string userId, string assetPairId, IEditMarkUp markUp)
        {
            var apiRecord = await _mmApiClient.GetMarkUp(assetPairId);
            var beforeJsonToLog = apiRecord.ToJson();

            apiRecord.Type = markUp.Type;
            apiRecord.AskMarkUp = decimal.Parse(markUp.AskMarkUp, CultureInfo.InvariantCulture);
            apiRecord.BidMarkUp = decimal.Parse(markUp.BidMarkUp, CultureInfo.InvariantCulture);

            await _mmApiClient.EditMarkUp(assetPairId, apiRecord);

            await _mmSettingsAuditLogRepository.InsertRecord(
                new MmSettingsAuditLogData
                {
                    AssetPairId = assetPairId,
                    UserId = userId,
                    CreatedTime = DateTime.UtcNow,
                    RecordType = MmSettingsAuditRecordType.EditMarkUp,
                    BeforeJson = beforeJsonToLog,
                    AfterJson = apiRecord.ToJson()
                });
        }        
    }
}
