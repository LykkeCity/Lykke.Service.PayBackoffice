using System;

namespace Core.MarginTrading.Models
{
    public interface IMaintenanceInfo
    {
        bool IsEnabled { get; }
        DateTime ChangedDate { get; }
        string ChangedReason { get; }
        string ChangedBy { get; }
    }
}
