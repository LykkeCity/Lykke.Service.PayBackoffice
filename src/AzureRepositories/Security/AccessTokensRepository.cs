using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.Security;

namespace AzureRepositories.Security
{
    public class AccessTokenRecord : BaseEntity
    {
        public static string GeneratePartition(string clientId)
        {
            return clientId;
        }

        public static string GenerateRowKey(string token)
        {
            return token;
        }

        public static AccessTokenRecord Create(string clientId, TokenType type)
        {
            return new AccessTokenRecord
            {
                PartitionKey = clientId,
                RowKey = Guid.NewGuid().ToString(),
                InsertionDt = DateTime.UtcNow,
                Type = type
            };
        }

        public TokenType Type { get; set; }
        public string ClientId => PartitionKey;
        public string Token => RowKey;
        public DateTime InsertionDt { get; set; }
    }

    public class AccessTokensRepository : IAccessTokensRepository
    {
        private readonly INoSQLTableStorage<AccessTokenRecord> _tableStorage;

        public AccessTokensRepository(INoSQLTableStorage<AccessTokenRecord> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<string> GenerateToken(string clientId, TokenType type)
        {
            var entity = AccessTokenRecord.Create(clientId, type);
            await _tableStorage.InsertAsync(entity);

            return entity.Token;
        }

        public async Task<TokenCheckResult> IsTokenValid(string clientId, string token, TokenType type)
        {
            var entity = await _tableStorage.GetDataAsync(AccessTokenRecord.GeneratePartition(clientId),
                AccessTokenRecord.GenerateRowKey(token));

            if (entity?.Type == type)
                return TokenCheckResult.Valid;

            return TokenCheckResult.Bad;
        }
    }
}
