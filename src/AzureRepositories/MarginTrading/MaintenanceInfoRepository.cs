using AzureStorage;
using Core.MarginTrading.Models;
using Core.MarginTrading.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureRepositories.MarginTrading
{
    public class MaintenanceInfoRepository : IMaintenanceInfoRepository
    {
        private readonly INoSQLTableStorage<MaintenanceInfoEntity> _tableStorage;

        public MaintenanceInfoRepository(INoSQLTableStorage<MaintenanceInfoEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IMaintenanceInfo> GetMaintenanceInfo(bool isDemo)
        {
            var rk = isDemo
                ? MaintenanceInfoEntity.GetDemoRowKey()
                : MaintenanceInfoEntity.GetLiveRowKey();

            return (IMaintenanceInfo)await _tableStorage.GetDataAsync(MaintenanceInfoEntity.GetPartitionKey(), rk) ??
                   new MaintenanceInfo();
        }

        public async Task SetMaintenanceInfo(IMaintenanceInfo record, bool isDemo)
        {
            await _tableStorage.InsertOrReplaceAsync(MaintenanceInfoEntity.CreateEntity(record, isDemo));
        }
    }
}
