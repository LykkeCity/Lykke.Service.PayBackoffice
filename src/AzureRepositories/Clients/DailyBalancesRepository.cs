using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Clients
{
    public class DailyBalances : TableEntity
    {
        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }

        public static string GenerateRowKey(DateTime day)
        {
            return $"{day.Year}-{day.Month:00}-{day.Day:00}";
        }

        public string ClientId { get; set; }
        public DateTime Day { get; set; }

        public IEnumerable<DailyBalanceRecord> BalanceRecords
        {
            get { return BalanceRecordsJson.DeserializeJson<IEnumerable<DailyBalanceRecord>>(); }
            set { BalanceRecordsJson = value.ToJson(); }
        }
        public string BalanceRecordsJson { get; set; }

        public static DailyBalances Create(string clientId, DateTime day, IEnumerable<DailyBalanceRecord> balances)
        {
            return new DailyBalances
            {
                BalanceRecords = balances,
                ClientId = clientId,
                Day = day,
                RowKey = GenerateRowKey(day),
                PartitionKey = GeneratePartitionKey(clientId)
            };
        }
    }

    public class DailyBalancesRepository : IDailyBalancesRepository
    {
        private readonly INoSQLTableStorage<DailyBalances> _tableStorage;

        public DailyBalancesRepository(INoSQLTableStorage<DailyBalances> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<DailyBalanceRecord>> GetDailyBalance(string clientId, DateTime day)
        {
            var entity =
                await
                    _tableStorage.GetDataAsync(DailyBalances.GeneratePartitionKey(clientId),
                        DailyBalances.GenerateRowKey(day));

            if (entity == null)
            {
                var all = await
                    _tableStorage.GetDataAsync(DailyBalances.GeneratePartitionKey(clientId));

                entity = all.Where(x => x.Day < day).OrderByDescending(x => x.Day).FirstOrDefault();
            }

            return entity?.BalanceRecords;
        }

        public Task InsertDailyBalance(string clientId, DateTime day, IEnumerable<DailyBalanceRecord> balances)
        {
            var entity = DailyBalances.Create(clientId, day, balances);
            return _tableStorage.InsertAsync(entity);
        }
    }
}
