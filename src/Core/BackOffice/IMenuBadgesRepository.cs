using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.BackOffice
{

    public static class MenuBadges
    {
        public const string Kyc = "KYC";
        public const string WithdrawRequest = "WithdrawRequest";
        public const string WithdrawRequestsHighVolume = "WithdrawRequestsHighVolume";
        public const string WithdrawRequestsLowVolume = "WithdrawRequestsLowVolume";
        public const string WithdrawRequestPostProcessing = "WithdrawRequestPostProcessing";
        public const string WithdrawRequestDocs = "WithdrawRequestDocs";
        public const string FailedTransaction = "FailedTransaction";
        public const string VoiceCallRequest = "VoiceCallRequest";
        public const string ActiveSessionCount = "ActiveSessionCount";
    }

    public interface IMenuBadge
    {
        string Id { get; }
        string Value { get; }
    }

    public class MenuBadge : IMenuBadge
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public interface IMenuBadgesRepository
    {
        Task SaveBadgeAsync(string id, string value);
        Task RemoveBadgeAsync(string id);
        Task<IEnumerable<IMenuBadge>> GetBadesAsync();
    }
}
