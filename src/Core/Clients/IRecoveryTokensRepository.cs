using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IAccessTokenLvl1
    {
        string ClientId { get; }
        string AccessToken { get; }
        DateTime IssueDateTime { get; }

        string EmailVerificationCode { get; }
        string PhoneVerificationCode { get; }
        DateTime? VerificationSendDateTime { get; }
    }

    [Flags]
    public enum RecoveryStatus
    {
        None = 0,
        InProcess = 1 << 0,
        Succeed = 1 << 1,
        Failed = 1 << 2,

        MessageCreated = 1 << 3 | InProcess,
        MessageSigned = 1 << 4 | InProcess,
    }

    public interface IRecoveryTokensRepository
    {
        Task SetRecoveryStatusAsync(string clientId, RecoveryStatus status);
        Task<Tuple<DateTime, RecoveryStatus>> GetRecoveryStatusAsync(string clientId);
        Task<IEnumerable<Tuple<string, DateTime, RecoveryStatus>>> GetAllRecoveryStatusesAsync();

        Task RegisterChallangeAsync(string clientId, string message);
        Task<string> GetChallangeAsync(string clientId);
        Task RemoveChallangeAsync(string clientId);

        Task RegisterAccessTokenLvl1Async(string clientId, string accessTokenLvl1);
        Task<IAccessTokenLvl1> GetAccessTokenLvl1Async(string accessTokenLvl1);
        Task RemoveAccessTokenLvl1Async(string accessTokenLvl1);
    }
}
