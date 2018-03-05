using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Users
{
    public interface IBackOfficeUser
    {
        string Id { get; }
        string FullName { get; }
        string[] Roles { get; }
        bool IsAdmin { get; }
        bool IsForceLogoutRequested { get; }
        bool IsDisabled { get; set; }
        string GoogleAuthenticatorPrivateKey { get; set; }
        bool GoogleAuthenticatorConfirmBinding { get; set; }
        bool UseTwoFactorVerification { get; set; }
        string TwoFactorVerificationTrustedTimeSpan { get; set; }
        string TwoFactorVerificationTrustedIPs { get; set; }
    }

    public class BackOfficeUser : IBackOfficeUser
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string[] Roles { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsForceLogoutRequested { get; set; }
        public bool IsDisabled { get; set; }
        public string GoogleAuthenticatorPrivateKey { get; set; }
        public bool GoogleAuthenticatorConfirmBinding { get; set; }
        public bool UseTwoFactorVerification { get; set; }
        public string TwoFactorVerificationTrustedTimeSpan { get; set; }
        public string TwoFactorVerificationTrustedIPs { get; set; }
    }

    public interface IBackOfficeUsersRepository
    {
        Task CreateAsync(string id, string fullName, bool isAdmin, string[] roles);
        Task UpdateAsync(string id, string fullName, bool isAdmin, string[] roles);
        Task<IBackOfficeUser> GetAsync(string id);
        Task<bool> UserExists(string id);
        Task<IEnumerable<IBackOfficeUser>> GetAllAsync();
        Task ForceLogoutAsync(string id);
        Task ResetForceLogoutAsync(string id);
        Task RemoveAsync(string id);
        Task DisableAsync(string id);
        Task EnableAsync(string id);
        Task GenerateGoogleAuthenticatorPrivateKey(string id);
        Task ResetGoogleAuthenticatorPrivateKey(string id);
        Task SetGoogleAuthenticatorConfirmBinding(string id, bool bindingState);
        Task SetUseTwoFactorVerification(string id, bool state);
        Task SetUseTwoFactorVerificationTrustedTimeSpan(string id, string timeSpan);
        Task SetTrustedIpAddresses(string id, string value);
    }

    public static class BackOfficeUserHelper
    {
        public static bool HasGoogleAuthenticatorPrivateKey(this IBackOfficeUser user)
        {
            return !string.IsNullOrEmpty(user.GoogleAuthenticatorPrivateKey);
        }
    }
}
