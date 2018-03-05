using System;
using System.Threading.Tasks;

namespace Core.MmSettings
{
    public enum MmSettingsAuditRecordType
    {
        EditAveragePriceMovement,
        EditMarkUp
    }

    public interface IMmSettingsAuditLogData
    {
        string AssetPairId { get; set; }
        string UserId { get; }
        DateTime CreatedTime { get; }
        MmSettingsAuditRecordType RecordType { get; }
        string BeforeJson { get; }
        string AfterJson { get; }        
    }

    public class MmSettingsAuditLogData : IMmSettingsAuditLogData
    {
        public string AssetPairId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedTime { get; set; }
        public MmSettingsAuditRecordType RecordType { get; set; }
        public string BeforeJson { get; set; }
        public string AfterJson { get; set; }        
    }

    public interface IMmSettingsAuditLogRepository
    {
        Task InsertRecord(IMmSettingsAuditLogData record);
    }
}
