using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Core.AuditLog
{
    public enum AuditRecordType
    {
        PersonalData,
        KycDocument,
        KycStatus,
        OtherEvent
    }

    public interface IAuditLogData
    {
        DateTime CreatedTime { get; }
        AuditRecordType RecordType { get; }
        string EventRecord { get; }
        string BeforeJson { get; }
        string AfterJson { get; }
        string Changer { get; }
    }

    public class AuditLogData : IAuditLogData
    {
        public DateTime CreatedTime { get; set; }
        public AuditRecordType RecordType { get; set; }
        public string EventRecord { get; set; }
        public string BeforeJson { get; set; }
        public string AfterJson { get; set; }
        public string Changer { get; set; }
    }

    public class ClientAuditLogData : IAuditLogData
    {
        public string ClientId { get; private set; }
        public DateTime CreatedTime { get; private set; }
        public AuditRecordType RecordType { get; private set; }
        public string EventRecord { get; private set; }
        public string BeforeJson { get; private set; }
        public string AfterJson { get; private set; }
        public string Changer { get; private set; }

        public ClientAuditLogData(IAuditLogData auditLogData, string clientId)
        {
            ClientId = clientId;
            CreatedTime = auditLogData.CreatedTime;
            RecordType = auditLogData.RecordType;
            EventRecord = auditLogData.EventRecord;
            BeforeJson = auditLogData.BeforeJson;
            AfterJson = auditLogData.AfterJson;
            Changer = auditLogData.Changer;
        }
    }

    public interface IAuditLogRepository
    {
        Task InsertRecord(string clientId, IAuditLogData record);
        Task<IEnumerable<IAuditLogData>> GetKycRecordsAsync(string id);
        Task<IEnumerable<ClientAuditLogData>> GetKycRecordsAsync(AuditRecordType recordType, DateTime from, DateTime to);
    }

    public static class AuditLogRepoExt
    {
        public static async Task AddAuditRecordAsync<T>(this IAuditLogRepository auditRepo, 
            string clientId, T objBefore, T objAfter, AuditRecordType type, string changer)
        {
            var auditRecord = new AuditLogData
            {
                BeforeJson = objBefore != null ? objBefore.ToJson() : null,
                AfterJson = objAfter != null ? objAfter.ToJson() : null,
                CreatedTime = DateTime.UtcNow,
                RecordType = type,
                Changer = changer
            };

            await auditRepo.InsertRecord(clientId, auditRecord);
        }

        public static async Task AddOtherEventAsync(this IAuditLogRepository auditRepo,
            string clientId, string eventRecord, string changer)
        {
            var auditRecord = new AuditLogData
            {
                EventRecord = eventRecord,
                CreatedTime = DateTime.UtcNow,
                RecordType = AuditRecordType.OtherEvent,
                Changer = changer
            };

            await auditRepo.InsertRecord(clientId, auditRecord);
        }
    }
}
