using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.AuditLog;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepositories.Mongo;

namespace MongoRepositories.Repositories.AuditLog
{
	public class AuditLogDataEntity : MongoEntity, IAuditLogData
	{
		private static string GenerateId(DateTime creationDt, string client, AuditRecordType recordType)
		{
			return IdGenerator.GenerateDateTimeIdNewFirst(creationDt) + "_" + $"AUD_{client}_{(int)recordType}";
		}

		public DateTime CreatedTime { get; set; }

		[BsonRepresentation(BsonType.String)]
		public AuditRecordType RecordType { get; set; }

		public string EventRecord { get; set; }

		public string BeforeJson { get; set; }
		public string AfterJson { get; set; }
		public string Changer { get; set; }

		public string ClientId { get; set; } //TODO : index with record type

		public static AuditLogDataEntity Create(string clientId, IAuditLogData data)
		{
			return new AuditLogDataEntity
			{
				BsonId = GenerateId(data.CreatedTime, clientId, data.RecordType),
				ClientId = clientId,
				RecordType = data.RecordType,
				CreatedTime = data.CreatedTime,
				BeforeJson = data.BeforeJson,
				AfterJson = data.AfterJson,
				EventRecord = data.EventRecord,
				Changer = data.Changer
			};
		}

	}

	public class AuditLogRepository : IAuditLogRepository
	{
		private IMongoStorage<AuditLogDataEntity> _tableStorage;

		public AuditLogRepository(IMongoStorage<AuditLogDataEntity> tableStorage)
		{
			_tableStorage = tableStorage;
		}

		public async Task InsertRecord(string clientId, IAuditLogData record)
		{
			var entity = AuditLogDataEntity.Create(clientId, record);
			await _tableStorage.InsertAsync(entity);
		}

		public async Task<IEnumerable<IAuditLogData>> GetKycRecordsAsync(string clientId)
		{
			var records = new List<AuditLogDataEntity>();
			var kycDocumentChangesTask =
				_tableStorage.GetDataAsync(x => x.ClientId == clientId && x.RecordType == AuditRecordType.KycDocument);
			var kycStatusChangesTask =
				_tableStorage.GetDataAsync(x => x.ClientId == clientId && x.RecordType == AuditRecordType.KycStatus);
			var kycPersonalDataTask =
				_tableStorage.GetDataAsync(x => x.ClientId == clientId && x.RecordType == AuditRecordType.PersonalData);
			var otherEventRecordsTask =
				_tableStorage.GetDataAsync(x => x.ClientId == clientId && x.RecordType == AuditRecordType.OtherEvent);

			records.AddRange(await kycDocumentChangesTask);
			records.AddRange(await kycStatusChangesTask);
			records.AddRange(await kycPersonalDataTask);
			records.AddRange(await otherEventRecordsTask);

			return records.OrderByDescending(x => x.CreatedTime);
        }

        public async Task<IEnumerable<ClientAuditLogData>> GetKycRecordsAsync(AuditRecordType recordType, DateTime from, DateTime to)
        {
            var kycStatusChanges = await _tableStorage.GetDataAsync(
                        item => item.RecordType == recordType && item.CreatedTime >= from && item.CreatedTime <= to);

            var clientLogEntities = kycStatusChanges.Select(logItem => new ClientAuditLogData(logItem, logItem.ClientId));

            return clientLogEntities;
        }
    }
}
