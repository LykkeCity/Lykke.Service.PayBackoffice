using AzureStorage;
using Core.CashOperations;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureRepositories.CashOperations
{
    public class WithdrawLimitRecord : TableEntity, IWithdrawLimit
    {
        public static string GeneratePartitionKey() => $"WithdrawLimit";
        public static string GenerateRowKey(string assetId) => assetId.ToUpper();
        public static WithdrawLimitRecord Create(string assetId, double limitAmount)
        {
            var limitRecord = new WithdrawLimitRecord()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(assetId),
                AssetId = assetId,
                LimitAmount = limitAmount,
                Timestamp = DateTime.UtcNow
            };

            return limitRecord;
        }


        public string AssetId { get; set; }
        public double LimitAmount { get; set; }

        public override string ToString() => $"{AssetId}: {LimitAmount}";
    }

    public class WithdrawLimitsRepository : IWithdrawLimitsRepository
    {
        private readonly INoSQLTableStorage<WithdrawLimitRecord> _tableStorage;
        private readonly double _defaultLimit;
        public double DefaultWithdrawalLimit { get { return _defaultLimit; } }

        public WithdrawLimitsRepository(INoSQLTableStorage<WithdrawLimitRecord> tableStorage, double defaultLimit)
        {
            _tableStorage = tableStorage;
            _defaultLimit = defaultLimit;
        }


        public async Task<bool> AddAsync(IWithdrawLimit item)
        {
            try
            {
                var record = WithdrawLimitRecord.Create(item.AssetId, item.LimitAmount);
                await _tableStorage.InsertOrReplaceAsync(record);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string assetId)
        {
            return await _tableStorage.DeleteIfExistAsync(WithdrawLimitRecord.GeneratePartitionKey(), WithdrawLimitRecord.GenerateRowKey(assetId));
        }

        public async Task<IEnumerable<IWithdrawLimit>> GetDataAsync()
        {
            var records = await _tableStorage.GetDataAsync(WithdrawLimitRecord.GeneratePartitionKey());
            return records ?? new List<WithdrawLimitRecord>();
        }
        
        public async Task<double> GetLimitByAssetAsync(string assetId)
        {
            var record = await _tableStorage.GetDataAsync(WithdrawLimitRecord.GeneratePartitionKey(), WithdrawLimitRecord.GenerateRowKey(assetId));
            if (record == null || record.LimitAmount <= 0)
                return _defaultLimit;
            
            return record.LimitAmount;
        }
    }
}
