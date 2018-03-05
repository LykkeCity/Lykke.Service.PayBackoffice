using System;

namespace Core.MarginTrading.Models
{
    public class MaintenanceInfo : IMaintenanceInfo
    {
        public bool IsEnabled { get; set; }
        public DateTime ChangedDate { get; set; }
        public string ChangedReason { get; set; }
        public string ChangedBy { get; set; }
    }
}
