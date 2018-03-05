using System;
using System.Threading.Tasks;

namespace Core.Security
{
    [Flags]
    public enum TokenType
    {
        SmsConfirmation = 1,
        EmailConfirmation = 2,
        PrivateKeyConfirmation = 4
    }

    public enum TokenCheckResult
    {
        Bad,
        Expired,
        Valid
    } 

    public interface IAccessTokensRepository
    {
        Task<string> GenerateToken(string clientId, TokenType type);
        Task<TokenCheckResult> IsTokenValid(string clientId, string token, TokenType type);
    }
}
