using Core.MarginTrading.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureRepositories.MarginTrading
{
    public class MaintenanceInfoEntity : TableEntity, IMaintenanceInfo
    {
        public static string GetPartitionKey()
        {
            return "MaintenanceInfo";
        }

        public static string GetDemoRowKey()
        {
            return "Demo";
        }

        public static string GetLiveRowKey()
        {
            return "Live";
        }

        public static MaintenanceInfoEntity CreateEntity(IMaintenanceInfo src, bool isDemo)
        {
            return new MaintenanceInfoEntity
            {
                ChangedBy = src.ChangedBy,
                ChangedDate = src.ChangedDate,
                ChangedReason = src.ChangedReason,
                IsEnabled = src.IsEnabled,
                PartitionKey = GetPartitionKey(),
                RowKey = isDemo
                    ? GetDemoRowKey()
                    : GetLiveRowKey()
            };
        }

        public bool IsEnabled { get; set; }
        public DateTime ChangedDate { get; set; }
        public string ChangedReason { get; set; }
        public string ChangedBy { get; set; }

        
    }
}
